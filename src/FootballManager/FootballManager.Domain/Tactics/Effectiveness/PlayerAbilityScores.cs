namespace FootballManager.Domain.Tactics.Effectiveness;

/// <summary>How good the players are in their roles: role suitability (familiarity) and capability (quality).</summary>
public class PlayerAbilityScores
{
    public Rating? Overall { get; set; }
    public Rating? RoleSuitability { get; set; }
    public Rating? Capability { get; set; }
    public EffectivenessAdvice? Advice { get; set; }
}
