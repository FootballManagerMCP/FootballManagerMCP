namespace FootballManager.Domain.Tactics;

/// <summary>
/// The tactic the team plays now, in full. Labels are in the game's display language; keys are the game's English
/// identifiers, as tactic edits take them.
/// </summary>
public class ActiveTactic
{
    public string? InPossessionFormation { get; set; }
    public string? OutOfPossessionFormation { get; set; }

    /// <summary>The in-possession formation's key; null for a custom shape.</summary>
    public string? InPossessionFormationKey { get; set; }

    /// <summary>The out-of-possession formation's key; null for a custom shape.</summary>
    public string? OutOfPossessionFormationKey { get; set; }

    public string? Mentality { get; set; }
    public string? MentalityKey { get; set; }

    /// <summary>The tactical style it was built from (e.g. "Gegenpress"), or the game's "from scratch" label.</summary>
    public string? Style { get; set; }

    public string? StyleKey { get; set; }

    /// <summary>Team instruction key to the option chosen, both the game's identifiers (e.g. "Tempo": "MuchHigherTempo").</summary>
    public Dictionary<string, string?> TeamInstructions { get; set; } = [];

    public List<LineupPosition> Lineup { get; set; } = [];
}
