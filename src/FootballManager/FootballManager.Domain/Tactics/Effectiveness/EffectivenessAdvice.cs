namespace FootballManager.Domain.Tactics.Effectiveness;

/// <summary>The assistant's comment on a part of the tactical effectiveness, in the game's display language.</summary>
public class EffectivenessAdvice
{
    public required string Text { get; set; }

    /// <summary>True for praise, false for a concern.</summary>
    public bool IsPositive { get; set; }
}
