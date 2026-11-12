using FootballManager.Domain.Competitions;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads competitions' stats screens in the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when
/// the game can't be reached.
/// </summary>
public interface ICompetitionsGameClient
{
    /// <summary>The user's league.</summary>
    Task<CompetitionInfo> GetUserLeagueAsync(CancellationToken cancellationToken = default);

    /// <summary>The user's first team's FM id.</summary>
    Task<int?> GetUserTeamIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The user's league and every league the game plays out in the nations of the user's continent, the leagues a
    /// stats screen can be read for.
    /// </summary>
    Task<List<CompetitionInfo>> GetCompetitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>One stat's table on the competition's stats screen, best first (the game keeps its top 100).</summary>
    Task<CompetitionStatTable> GetStatTableAsync(int competitionId, CompetitionStat stat, CancellationToken cancellationToken = default);

    /// <summary>
    /// The players at <paramref name="rows"/> of the competition's tables, as the user's scouting knows them and with
    /// their contracts, in the order given.
    /// </summary>
    Task<List<CompetitionPlayerProfile>> GetPlayersAsync(
        int competitionId, IReadOnlyList<CompetitionRowReference> rows, CancellationToken cancellationToken = default);
}
