using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Entities;

/// <summary>
/// The game's own assessment of how well a tactic suits the squad (its "tactical effectiveness"), 0-100. Overall
/// combines player ability, player buy-in and tactical understanding; each breaks down further.
/// </summary>
public class TacticalEffectiveness
{
    public Rating? Overall { get; set; }
    public PlayerAbilityScores? PlayerAbility { get; set; }
    public PlayerBuyInScores? PlayerBuyIn { get; set; }
    public TacticalUnderstandingScores? TacticalUnderstanding { get; set; }
    public NextMatchScores? NextMatch { get; set; }

    /// <summary>The game's warnings that are raised, e.g. "PlayerBelowThresholdWarning".</summary>
    public List<string> Warnings { get; set; } = [];
}
