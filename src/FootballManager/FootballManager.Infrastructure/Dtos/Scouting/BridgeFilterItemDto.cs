namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>A value a search rule can take: its label and the game's id for it (a position mask, an age, a grade).</summary>
internal sealed class BridgeFilterItemDto
{
    public string? Name { get; set; }

    // Role masks use all 64 bits, "any" is often 2^32 - 1.
    public decimal? Id { get; set; }
}
