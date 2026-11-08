using FootballManager.Domain.Matches;
using FootballManager.Domain.Players;

namespace FootballManager.Domain.Teams;

/// <summary>
/// A scouting report on a team, from its competitive matches this season: its record, how it plays home and away, how
/// it lines up, its squad, and every match.
/// </summary>
public class TeamReport
{
    public int TeamId { get; set; }
    public required string TeamName { get; set; }

    public required MatchRecord Overall { get; set; }
    public required MatchRecord Home { get; set; }
    public required MatchRecord Away { get; set; }

    public required StyleProfile Style { get; set; }
    public required StyleProfile HomeStyle { get; set; }
    public required StyleProfile AwayStyle { get; set; }

    public required UsualLineup Lineup { get; set; }
    public List<Player> Squad { get; set; } = [];

    /// <summary>Competitive matches played, oldest first, seen from the team's side.</summary>
    public List<MatchSummary> Played { get; set; } = [];

    public List<MatchSummary> Upcoming { get; set; } = [];
}
