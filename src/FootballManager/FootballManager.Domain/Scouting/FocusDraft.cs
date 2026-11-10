using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Scouting;

/// <summary>A new recruitment focus checked against the game: every value one the game offers.</summary>
public class FocusDraft
{
    public required string Name { get; set; }
    public List<ScoutingArea> Areas { get; set; } = [];

    /// <summary>Empty means any position.</summary>
    public List<TacticalPosition> Positions { get; set; } = [];

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int? MinAbilityStars { get; set; }
    public int? MinPotentialStars { get; set; }
}
