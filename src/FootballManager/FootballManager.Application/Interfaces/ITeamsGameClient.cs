using FootballManager.Domain.Teams;

namespace FootballManager.Application.Interfaces;

/// <summary>Reads other teams from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it can't.</summary>
public interface ITeamsGameClient
{
    /// <summary>The teams the game can be asked about: the user's league, then every competition the user plays in.</summary>
    Task<List<KnownTeam>> GetTeamsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// A club the game's search has listed (<see cref="IDatabaseSearchGameClient"/>), as itself or as a person's club,
    /// by its FM id: its first team. Null when no search has listed it.
    /// </summary>
    Task<KnownTeam?> GetSearchedTeamAsync(int clubId, CancellationToken cancellationToken = default);

    /// <summary>A team's season, by its FM id (one of <see cref="GetTeamsAsync"/>'s or <see cref="GetSearchedTeamAsync"/>'s).</summary>
    Task<TeamSeason> GetSeasonAsync(int teamId, CancellationToken cancellationToken = default);
}
