using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Entities;

/// <summary>The outlook against the next opponent, given how much is known about them.</summary>
public class NextMatchScores
{
    public Rating? Overall { get; set; }
    public EffectivenessAdvice? Advice { get; set; }
}
