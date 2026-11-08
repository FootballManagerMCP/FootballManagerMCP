using FootballManager.Domain.Matches;

namespace FootballManager.Domain.Teams;

/// <summary>One of a team's fixtures, seen from its side, with both teams' numbers and its lineup once played.</summary>
public class TeamMatch
{
    public required MatchSummary Summary { get; set; }

    /// <summary>The team's numbers; null until played, or when the game only simulated the result.</summary>
    public TeamMatchStats? Stats { get; set; }

    public TeamMatchStats? OpponentStats { get; set; }

    /// <summary>The team's players, starters first, with their positions and roles.</summary>
    public List<TeamLineupEntry> Lineup { get; set; } = [];
}
