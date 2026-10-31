using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Entities;

/// <summary>How much the players believe in the approach, and why.</summary>
public class PlayerBuyInScores
{
    public Rating? Overall { get; set; }
    public Rating? ManagerSupport { get; set; }
    public Rating? ManagerTendencies { get; set; }
    public Rating? Relationships { get; set; }
    public EffectivenessAdvice? Advice { get; set; }
}
