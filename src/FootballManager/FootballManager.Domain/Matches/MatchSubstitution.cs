using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Matches;

/// <summary>A substitution of the user's team: who left the pitch, who came on in his place, and when.</summary>
public class MatchSubstitution
{
    public int? Minute { get; set; }
    public int? OffPlayerId { get; set; }
    public string? Off { get; set; }
    public int? OnPlayerId { get; set; }
    public string? On { get; set; }

    /// <summary>The position with the ball of the slot the substitute took; unknown when the pairing is.</summary>
    public TacticalPosition? Position { get; set; }

    /// <summary>The player who came off was injured.</summary>
    public bool Injury { get; set; }

    /// <summary>
    /// Several players went off and came on in the same minute, and the game's data doesn't say which one replaced
    /// which: who went off and who came on is right, the pairing may not be.
    /// </summary>
    public bool Uncertain { get; set; }
}
