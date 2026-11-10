namespace FootballManager.Domain.Scouting;

/// <summary>A team's squad, as the club's scouting can act on it: each player with whether he's shortlisted or watched.</summary>
public class TeamPlayers
{
    public int TeamId { get; set; }
    public required string TeamName { get; set; }
    public List<ScoutedPlayer> Players { get; set; } = [];
}
