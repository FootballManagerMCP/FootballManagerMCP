using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the user's scouting from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it
/// can't.
/// </summary>
public interface IScoutingGameClient
{
    /// <summary>The scouting centre's counts, the scouting budget, the club's scouts and its recruitment focuses.</summary>
    Task<ScoutingOverview> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>The season's scouting budget and what scouting has cost so far.</summary>
    Task<ScoutingBudget> GetBudgetAsync(CancellationToken cancellationToken = default);

    /// <summary>The club's recruitment focuses.</summary>
    Task<List<RecruitmentFocus>> GetFocusesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The players of a scouting list, as the club's scouting knows them: the scouting centre's recommendations, the
    /// shortlist, a focus's <paramref name="pool"/> (<paramref name="focusId"/> being one of
    /// <see cref="GetFocusesAsync"/>'s), or the game's player search with the search screen's state. Each comes with the
    /// given <paramref name="attributes"/>, current and potential ability when <paramref name="includeAbility"/> is set,
    /// and the latest report when <paramref name="includeReport"/> is. Only what is asked for is fetched, and only the
    /// list's first <paramref name="take"/> players when that is given (in the game's order), with the list's size.
    /// </summary>
    Task<ScoutedPlayerPage> GetPlayersAsync(
        ScoutingSource source,
        int? focusId,
        FocusPool pool,
        IReadOnlyCollection<AttributeDefinition> attributes,
        bool includeAbility,
        bool includeReport,
        int? take = null,
        CancellationToken cancellationToken = default);
}
