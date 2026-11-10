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

    /// <summary>When a substitute came on. Unknown for one who came on and left again, unless his slot can be traced.</summary>
    public int? MinuteOn { get; set; }

    /// <summary>When the player left the pitch: substituted, injured or sent off. Null when he played to the end.</summary>
    public int? MinuteOff { get; set; }

    /// <summary>He left the pitch injured.</summary>
    public bool WentOffInjured { get; set; }

    public bool YellowCard { get; set; }
    public bool RedCard { get; set; }
    public double? Rating { get; set; }

    /// <summary>A substitute's are those of the slot he took, the player he replaced's.</summary>
    public MatchPosition InPossession { get; set; } = new();

    public MatchPosition OutOfPossession { get; set; } = new();
    public PlayerMatchStats? Stats { get; set; }
}
