namespace FootballManager.Domain.Tactics.Editing;

/// <summary>Everything a tactic can be set to, by key, with what the active tactic holds now.</summary>
public class TacticOptions
{
    /// <summary>The slot the team plays now, 1 to 3.</summary>
    public int ActiveSlot { get; set; }

    public string? Style { get; set; }
    public List<TacticOption> Styles { get; set; } = [];

    public string? Mentality { get; set; }
    public List<TacticOption> Mentalities { get; set; } = [];

    /// <summary>The formation of each phase now; null for a custom shape.</summary>
    public ByPhase<string> Formation { get; set; } = new();

    /// <summary>The formations the game offers for each phase.</summary>
    public ByPhase<List<TacticOption>> Formations { get; set; } = new();

    public List<TeamInstructionOptions> TeamInstructions { get; set; } = [];
    public List<PlayerInstructionGroup> PlayerInstructions { get; set; } = [];

    /// <summary>Every role key of each phase. Which roles a position offers comes with the position (role fit).</summary>
    public ByPhase<List<string>> Roles { get; set; } = new();
}
