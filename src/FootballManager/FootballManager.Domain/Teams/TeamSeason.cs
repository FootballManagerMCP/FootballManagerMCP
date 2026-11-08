using FootballManager.Domain.Players;

namespace FootballManager.Domain.Teams;

/// <summary>A team's season as the game holds it: every fixture seen from its side, and its squad.</summary>
public class TeamSeason
{
    public int TeamId { get; set; }
    public required string TeamName { get; set; }
    public List<TeamMatch> Matches { get; set; } = [];

    /// <summary>The first team, with what the user's scouting knows of each player.</summary>
    public List<Player> Squad { get; set; } = [];
}
