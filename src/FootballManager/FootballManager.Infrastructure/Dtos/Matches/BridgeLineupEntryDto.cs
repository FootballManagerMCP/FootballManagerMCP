namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>A player of the user's team in a past match, with their position and role in one phase.</summary>
internal sealed class BridgeLineupEntryDto
{
    public BridgeReferenceDto? Player { get; set; }

    /// <summary>A position mask (see PositionMasks).</summary>
    public ulong? Position { get; set; }

    /// <summary>A role mask, numbered separately in and out of possession.</summary>
    public ulong? PositionRole { get; set; }

    /// <summary>0-10 for the starting eleven, higher for substitutes.</summary>
    public int? TeamSelectionIndex { get; set; }
    public bool? MatchWasSubbedOn { get; set; }
    public double? MatchRating { get; set; }

    /// <summary>Above 0 when the player was injured in the match.</summary>
    public int? MatchInjuryType { get; set; }
}
