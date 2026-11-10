using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Matches;

/// <summary>A substitution of the user's team: who left a slot, who took it, and when.</summary>
public class MatchSubstitution
{
    public int? Minute { get; set; }
    public int? OffPlayerId { get; set; }

    /// <summary>Null for a player who has since left the squad.</summary>
    public string? Off { get; set; }

    public int? OnPlayerId { get; set; }
    public string? On { get; set; }

    /// <summary>The slot's position with the ball.</summary>
    public TacticalPosition? Position { get; set; }

    /// <summary>The player who came off was injured.</summary>
    public bool Injury { get; set; }
}
