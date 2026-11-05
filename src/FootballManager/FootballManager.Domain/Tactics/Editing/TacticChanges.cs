namespace FootballManager.Domain.Tactics.Editing;

/// <summary>
/// What to change in a tactic slot, by key. Anything left out stays as it is. The game applies them in its own order:
/// style, mentality, team instructions, formations, then the lineup.
/// </summary>
public class TacticChanges
{
    /// <summary>The slot to write, 1 to 3. Writing makes it the slot the team plays.</summary>
    public int Slot { get; set; }

    public string? Style { get; set; }
    public string? Mentality { get; set; }

    /// <summary>Team instruction key to option key; the others keep their options.</summary>
    public Dictionary<string, string> TeamInstructions { get; set; } = [];

    public ByPhase<string>? Formation { get; set; }

    /// <summary>Players of the starting eleven to place. Naming a player not in it needs all eleven.</summary>
    public List<LineupChange> Lineup { get; set; } = [];
}
