namespace FootballManager.Domain.Tactics.Effectiveness;

/// <summary>The squad's understanding of one area of a tactic (e.g. its pressing intensity), and where it comes from.</summary>
public class TacticalArea
{
    public Rating? Understanding { get; set; }
    public Rating? MatchExposure { get; set; }
    public Rating? TrainingContribution { get; set; }
}
