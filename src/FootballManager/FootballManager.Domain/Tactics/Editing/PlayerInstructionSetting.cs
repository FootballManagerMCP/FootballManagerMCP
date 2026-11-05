namespace FootballManager.Domain.Tactics.Editing;

/// <summary>
/// The option a player instruction group holds for a lineup slot, and whether the slot's role or position locks it
/// (e.g. a defensive midfielder always holds position).
/// </summary>
public sealed record PlayerInstructionSetting(string? Option, bool Locked);
