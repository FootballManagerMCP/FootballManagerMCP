namespace FootballManager.Domain.Matches;

/// <summary>A player who took part in a match for the user's team: a starter, or a substitute who came on.</summary>
public class MatchLineupPlayer
{
    /// <summary>0-10 for the starting eleven, as the game numbers lineup slots; higher for substitutes.</summary>
    public int SelectionIndex { get; set; }
    public int? PlayerId { get; set; }

    /// <summary>Null for a player who has since left the squad.</summary>
    public string? Name { get; set; }
    public bool CameOnAsSubstitute { get; set; }
    public double? Rating { get; set; }
    public MatchPosition InPossession { get; set; } = new();
    public MatchPosition OutOfPossession { get; set; } = new();
    public PlayerMatchStats? Stats { get; set; }
}
