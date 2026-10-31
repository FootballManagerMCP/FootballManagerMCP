using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Entities;

/// <summary>How well the squad knows the tactic, overall and by area; grown through matches and training.</summary>
public class TacticalUnderstandingScores
{
    public Rating? Overall { get; set; }

    /// <summary>The players' general grasp of tactical concepts.</summary>
    public Rating? Comprehension { get; set; }
    public EffectivenessAdvice? ComprehensionAdvice { get; set; }

    public TacticalArea? InPossessionShape { get; set; }
    public TacticalArea? OutOfPossessionShape { get; set; }
    public TacticalArea? BuildUpStrategy { get; set; }
    public TacticalArea? ChanceCreation { get; set; }
    public TacticalArea? PassingStyle { get; set; }
    public TacticalArea? PressingIntensity { get; set; }
    public TacticalArea? PressingTrap { get; set; }
    public TacticalArea? LineOfEngagement { get; set; }
    public EffectivenessAdvice? AreasAdvice { get; set; }

    public TacticalComplexity? Complexity { get; set; }
}
