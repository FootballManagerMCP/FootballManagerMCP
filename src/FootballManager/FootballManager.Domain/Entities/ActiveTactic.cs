namespace FootballManager.Domain.Entities;

/// <summary>The tactic the team plays now, in full. Text values are in the game's display language.</summary>
public class ActiveTactic
{
    public string? InPossessionFormation { get; set; }
    public string? OutOfPossessionFormation { get; set; }
    public string? Mentality { get; set; }

    /// <summary>The tactical style it was built from (e.g. "Gegenpress"), or the game's "from scratch" label.</summary>
    public string? Style { get; set; }

    /// <summary>Team instruction to the option chosen, by the game's own names (e.g. "Tempo": "MuchHigherTempo").</summary>
    public Dictionary<string, string?> TeamInstructions { get; set; } = [];

    public List<LineupPosition> Lineup { get; set; } = [];
}
