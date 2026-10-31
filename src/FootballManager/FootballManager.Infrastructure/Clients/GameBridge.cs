using System.Net.Http.Json;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Infrastructure.Dtos;
using Microsoft.Extensions.Logging;

namespace FootballManager.Infrastructure.Clients;

// The HTTP side of the MCPFM plugin inside the game: queries go out, the game's values come back as JSON. The plugin is
// a generic pipe; which paths make up a player or a tactic lives in the clients using this.
internal sealed class GameBridge(IHttpClientFactory httpClientFactory, ILogger<GameBridge> logger)
{
    public const string HttpClientName = "GameBridge";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<BridgeRecordDto<TValues>> GetRecordAsync<TValues>(BridgeQuery query, CancellationToken cancellationToken)
        where TValues : new()
    {
        var record = await GetAsync<BridgeRecordDto<TValues>>(query.ToString(), cancellationToken);
        LogUnanswered([record]);
        return record;
    }

    /// <param name="gapsExpected">
    /// Set when the game is expected to leave some values out (e.g. role ratings for players who can't play a position),
    /// so they aren't logged as problems.
    /// </param>
    public async Task<BridgeListDto<TValues>> GetListAsync<TValues>(
        BridgeQuery query, CancellationToken cancellationToken, bool gapsExpected = false) where TValues : new()
    {
        var list = await GetAsync<BridgeListDto<TValues>>(query.ToString(), cancellationToken);

        if (!gapsExpected)
        {
            LogUnanswered(list.Items);
        }

        return list;
    }

    /// <summary>The user's first-team players, one record each; "contexts" passes the user's manager and team.</summary>
    public async Task<BridgeListDto<TValues>> GetSquadAsync<TValues>(IEnumerable<string> paths, CancellationToken cancellationToken)
        where TValues : new()
    {
        var squad = await GetAsync<BridgeListDto<TValues>>(
            $"/squad?contexts=true&props={Uri.EscapeDataString(string.Join(',', paths))}", cancellationToken);

        LogUnanswered(squad.Items);
        return squad;
    }

    /// <summary>The game's names for ids it uses as values, e.g. a team instruction's option ("MuchHigherTempo").</summary>
    public async Task<Dictionary<string, string?>> GetNamesAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken)
    {
        var list = string.Join(',', ids.Distinct());

        return list.Length == 0
            ? []
            : await GetAsync<Dictionary<string, string?>>($"/names?ids={list}", cancellationToken);
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        var http = httpClientFactory.CreateClient(HttpClientName);
        HttpResponseMessage response;

        try
        {
            response = await http.GetAsync(path, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new GameUnavailableException(
                $"Football Manager isn't answering on {http.BaseAddress}: is the game running, with the MCPFM plugin installed?",
                exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new GameUnavailableException("The game didn't answer in time (it may be busy processing).", exception);
        }

        if (!response.IsSuccessStatusCode)
        {
            // The plugin answers errors in plain text, its first line being the reason (e.g. no save loaded yet).
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new GameUnavailableException($"The game couldn't answer ({(int)response.StatusCode}): {body.Split('\n')[0].Trim()}");
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new GameUnavailableException("The game sent an empty answer.");
    }

    private void LogUnanswered<TValues>(IReadOnlyCollection<BridgeRecordDto<TValues>> records) where TValues : new()
    {
        var pending = records.SelectMany(r => r.Pending ?? []).Distinct().ToList();
        var errors = records.SelectMany(r => r.Errors ?? []).Select(e => $"{e.Key}: {e.Value}").Distinct().ToList();

        if (pending.Count > 0)
        {
            logger.LogWarning("The game didn't answer {Paths} for some records", pending);
        }

        if (errors.Count > 0)
        {
            logger.LogWarning("The game rejected {Errors}", errors);
        }
    }
}
