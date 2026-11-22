namespace FootballManager.Infrastructure.Mappings;

/// <summary>
/// A substitution in a past match: the starting lineup slot of the player leaving (unknown for a substitute who
/// couldn't be traced), the minute when known, the players leaving and coming on by their place in the game's
/// database, and whether the pairing is a guess among several changes in the same minute.
/// </summary>
internal sealed record SlotChange(int? Slot, int? Minute, int OffPlayer, int OnPlayer, bool Uncertain = false);
