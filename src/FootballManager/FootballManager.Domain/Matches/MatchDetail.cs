namespace FootballManager.Domain.Matches;

/// <summary>One match in depth, from the user's side. Only the summary is known before it's played.</summary>
public class MatchDetail
{
    public required MatchSummary Summary { get; set; }
    public TeamMatchStats? TeamStats { get; set; }
    public TeamMatchStats? OpponentStats { get; set; }
    public List<MatchGoal> Goals { get; set; } = [];

    /// <summary>The user's players who took part, starters first, with their positions, roles and numbers.</summary>
    public List<MatchLineupPlayer> Lineup { get; set; } = [];
    public PlayerOfTheMatch? PlayerOfTheMatch { get; set; }
}
