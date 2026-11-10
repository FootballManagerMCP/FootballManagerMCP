using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Database;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Database;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Searches the game's database through the plugin, by typing the text into the game's search box and reading its
// results, as the box lists them under itself. Each player and club read is remembered with its place, so other
// clients can reach it later (LocateAsync).
internal sealed class DatabaseSearchBridgeClient(GameBridge bridge, SearchedRecords searched) : IDatabaseSearchGameClient
{
    // The results are in by the time the write has read the text back; this covers a slow frame.
    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan SettlePoll = TimeSpan.FromMilliseconds(200);

    public async Task<DatabaseSearch> SearchAsync(
        string text, IReadOnlyCollection<DatabaseRecordKind> kinds, int take, CancellationToken cancellationToken = default)
    {
        await TypeAsync(text, cancellationToken);

        // Kept to some kinds, the results are looked through further down for enough of them.
        var (records, total) = await ReadAsync(text, kinds.Count == 0 ? take : DatabaseSearchConstants.MaxRead, cancellationToken);

        return new DatabaseSearch
        {
            Query = text,
            Total = total,
            Read = records.Count,
            Results = records.Where(record => kinds.Count == 0 || kinds.Contains(record.Kind)).Take(take).ToList()
        };
    }

    /// <summary>
    /// The path, on the game root, to a player or club a search listed (a club also as a person's): under the search
    /// box's results, searched for again when the box has moved on. Null when no search has listed it, or it's no
    /// longer found.
    /// </summary>
    public async Task<string?> LocateAsync(DatabaseRecordKind kind, int id, CancellationToken cancellationToken = default)
    {
        if (searched.Of(kind, id) is not { } place)
        {
            return null;
        }

        if (await LeadsToAsync(place, id, cancellationToken))
        {
            return PathOf(place);
        }

        await TypeAsync(place.Query, cancellationToken);
        await ReadAsync(place.Query, DatabaseSearchConstants.MaxRead, cancellationToken);

        // Not among the results any more, it's still remembered at its old place, which no longer leads to it.
        return searched.Of(kind, id) is { } again && await LeadsToAsync(again, id, cancellationToken)
            ? PathOf(again)
            : null;
    }

    // The results from the top, remembering where each player and club stands.
    private async Task<(List<DatabaseRecord> Records, int Total)> ReadAsync(string text, int take, CancellationToken cancellationToken)
    {
        var page = await bridge.GetListAsync<BridgeDatabaseRecordDto>(
            OnGame().List(DatabaseSearchConstants.Results).Take(take).Props(DatabaseSearchConstants.ResultProperties),
            cancellationToken,
            gapsExpected: true);

        var records = page.Items.Select(item => item.Values.Adapt<DatabaseRecord>()).ToList();

        for (var position = 0; position < records.Count; position++)
        {
            if (records[position] is { Kind: DatabaseRecordKind.Player or DatabaseRecordKind.Club, Id: { } id } record)
            {
                searched.Remember(record.Kind, id, new SearchedPlace(text, position, DatabaseSearchConstants.Record));
            }

            if (records[position].ClubId is { } clubId)
            {
                searched.Remember(DatabaseRecordKind.Club, clubId, new SearchedPlace(text, position, DatabaseSearchConstants.PersonClub));
            }
        }

        return (records, page.Total ?? page.Count);
    }

    private async Task<bool> LeadsToAsync(SearchedPlace place, int id, CancellationToken cancellationToken)
    {
        try
        {
            var record = await bridge.GetRecordAsync<BridgeRecordIdDto>(
                OnGame().Base(PathOf(place)).Props([GameConstants.UniqueId]), cancellationToken);

            return record.Values.Id == id;
        }
        catch (GameUnavailableException) when (!cancellationToken.IsCancellationRequested)
        {
            // The box lists fewer results now than that place.
            return false;
        }
    }

    // Types the text into the box, and waits for the game to have searched for it.
    private async Task TypeAsync(string text, CancellationToken cancellationToken)
    {
        var write = await bridge.WriteAsync<BridgeWriteResultDto>(
            OnSearch().ToWrite("/set", ("path", DatabaseSearchConstants.Text), ("value", $"'{text}'")),
            cancellationToken);

        // As the box holds it, should the game shorten or tidy the text.
        var typed = write.After is { ValueKind: JsonValueKind.String } after ? after.GetString() : text;
        var deadline = DateTime.UtcNow + SettleTimeout;

        while (true)
        {
            var search = await bridge.GetRecordAsync<BridgeSearchProgressDto>(
                OnSearch().Props([DatabaseSearchConstants.Text, DatabaseSearchConstants.IsFinished]),
                cancellationToken);

            if (search.Values.Text == typed && search.Values.IsFinished == true)
            {
                return;
            }

            if (DateTime.UtcNow > deadline)
            {
                throw new GameRejectedException(search.Values.Text == typed
                    ? $"The game didn't finish searching for '{text}'."
                    : $"The game's search box holds '{search.Values.Text}', not '{text}'.");
            }

            await Task.Delay(SettlePoll, cancellationToken);
        }
    }

    private static string PathOf(SearchedPlace place)
    {
        return $"{DatabaseSearchConstants.Result(place.Position)}.{place.Path}";
    }

    private static BridgeQuery OnSearch()
    {
        return OnGame().Base(DatabaseSearchConstants.Search);
    }

    private static BridgeQuery OnGame()
    {
        return BridgeQuery.FromGame().WithHumanAndTeam();
    }
}
