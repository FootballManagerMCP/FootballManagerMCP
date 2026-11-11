namespace FootballManager.Domain.Tactics.Editing;

/// <summary>A substitute's place in the matchday squad, and who holds it.</summary>
public class BenchPlace
{
    /// <summary>The game's team selection index: 11 for the bench's first place, one more for each next.</summary>
    public int Index { get; set; }

    /// <summary>Null for an empty place.</summary>
    public int? PlayerId { get; set; }

    public string? PlayerName { get; set; }
}
