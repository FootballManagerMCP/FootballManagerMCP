namespace FootballManager.Domain.Tactics.Effectiveness;

/// <summary>How demanding a tactic is to learn, from its team, player and opposition instructions.</summary>
public class TacticalComplexity
{
    public Rating? Overall { get; set; }
    public Rating? TeamInstructions { get; set; }
    public Rating? PlayerInstructions { get; set; }
    public Rating? OppositionInstructions { get; set; }
}
