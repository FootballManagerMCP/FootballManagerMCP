namespace FootballManager.Infrastructure.Mappings;

/// <summary>
/// A substitution in a past match: the lineup slot, the minute when known, and the players leaving and taking it, by
/// their place in the game's database.
/// </summary>
internal sealed record SlotChange(int Slot, int? Minute, int OffPlayer, int OnPlayer);
