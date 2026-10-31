namespace FootballManager.Domain.Tactics;

/// <summary>A slot on the tactics pitch: a position, and for central slots which side of the line it's on.</summary>
public sealed record TacticalPosition(PitchPosition Position, PositionSide Side = PositionSide.Centre)
{
    /// <summary>FM's abbreviation with the side appended, e.g. "DC", "DCL" or "MCR".</summary>
    public string Code => Side switch
    {
        PositionSide.Left => $"{Position}L",
        PositionSide.Right => $"{Position}R",
        _ => Position.ToString()
    };
}
