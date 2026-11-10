using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Services;

public interface IScoutingService
{
    /// <summary>The scouting centre's counts, the club's scouts and its recruitment focuses.</summary>
    Task<Result<ScoutingOverview>> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The players of one of the user's scouting lists, as the club's scouting knows them, best graded first. A focus
    /// is named by its id (from the overview). The player search holds hundreds of players or more: only its first
    /// <paramref name="limit"/> are read (<see cref="ScoutingService.DefaultSearchLimit"/> by default), in the search screen's order.
    /// </summary>
    Task<Result<ScoutedPlayerList>> GetPlayersAsync(
        ScoutingSource source = ScoutingSource.Recommendations,
        int? focusId = null,
        FocusPool pool = FocusPool.Recommended,
        bool includeAttributes = false,
        bool includeHidden = false,
        bool includeReport = false,
        int? limit = null,
        CancellationToken cancellationToken = default);
}

public class ScoutingService(IScoutingGameClient gameClient) : IScoutingService
{
    public const int DefaultSearchLimit = 50;
    public const int MaxLimit = 200;

    public async Task<Result<ScoutingOverview>> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await gameClient.GetOverviewAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ScoutingOverview>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<ScoutedPlayerList>> GetPlayersAsync(
        ScoutingSource source = ScoutingSource.Recommendations,
        int? focusId = null,
        FocusPool pool = FocusPool.Recommended,
        bool includeAttributes = false,
        bool includeHidden = false,
        bool includeReport = false,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        if (source == ScoutingSource.Focus && focusId is null)
        {
            return Fail("focusId", "A focus's players need the focus's id (get_scouting lists the focuses).");
        }

        if (source != ScoutingSource.Focus && focusId is not null)
        {
            return Fail("focusId", "A focus id only goes with the focus source.");
        }

        if (limit is < 1 or > MaxLimit)
        {
            return Fail("limit", $"The limit runs 1 to {MaxLimit}.");
        }

        // Hidden values are never fetched unless asked for, so they can't leak into an answer by accident.
        var attributes = new List<AttributeDefinition>();

        if (includeAttributes)
        {
            attributes.AddRange(PlayerAttributes.Visible);
        }

        if (includeHidden)
        {
            attributes.AddRange(PlayerAttributes.Hidden);
        }

        try
        {
            RecruitmentFocus? focus = null;

            if (focusId is { } id)
            {
                var focuses = await gameClient.GetFocusesAsync(cancellationToken);
                focus = focuses.FirstOrDefault(f => f.Id == id);

                if (focus is null)
                {
                    var known = string.Join(", ", focuses.Select(f => $"{f.Name} (id {f.Id})"));
                    return Fail("focusId", $"No recruitment focus with id {id}. The club's focuses: {(known.Length > 0 ? known : "none")}.");
                }

                if (focus.ShowsRemovedFocus)
                {
                    return Fail("focusId", $"The game still shows the removed focus '{focus.Name}' under id {id}, which it has " +
                                           "given a newer focus: its players would be the removed focus's. They can be read " +
                                           "once the game refreshes its focus list.");
                }
            }

            // The search is read a page at a time, in its own order; the other lists are short, sorted whole, then cut.
            var take = source == ScoutingSource.Search ? limit ?? DefaultSearchLimit : (int?)null;
            var page = await gameClient.GetPlayersAsync(
                source, focusId, pool, attributes, includeHidden, includeReport, take, cancellationToken);

            // The sort is stable: within a grade, players keep the game's own order.
            var players = page.Players.OrderByDescending(p => p.Grade ?? -1).Take(limit ?? int.MaxValue).ToList();

            return Result.Ok(new ScoutedPlayerList
            {
                Source = source,
                Focus = focus,
                Pool = focus is null ? null : pool,
                Search = page.Search,
                Total = page.Total,
                Players = players,
                HiddenDataMasked = includeHidden && players.Count > 0 && !players.Any(p => p.HasHiddenValues)
            });
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ScoutedPlayerList>(new GameUnavailableError(exception.Message));
        }
    }

    private static Result<ScoutedPlayerList> Fail(string property, string message)
    {
        return Result.Fail<ScoutedPlayerList>(new ValidationError(property, [message]));
    }
}
