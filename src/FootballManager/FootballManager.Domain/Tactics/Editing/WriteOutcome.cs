namespace FootballManager.Domain.Tactics.Editing;

/// <summary>A setting before and after a write, by key, as the game reads back.</summary>
public sealed record WriteOutcome(string? Before, string? After);
