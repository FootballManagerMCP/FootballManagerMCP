namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>One box of a recruitment focus's position grid: a position mask, its phase, and whether it's ticked.</summary>
internal sealed class BridgeFocusPositionDto
{
    public ulong? Position { get; set; }
    public bool? IsInPossession { get; set; }
    public int? Ticked { get; set; }
}
