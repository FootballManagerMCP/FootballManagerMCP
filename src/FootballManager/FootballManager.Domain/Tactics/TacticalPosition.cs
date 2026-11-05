namespace FootballManager.Domain.Tactics;

/// <summary>A slot on the tactics pitch: a position, and for central slots which side of the line it's on.</summary>
public sealed record TacticalPosition(PitchPosition Position, PositionSide Side = PositionSide.Centre)
{
    // The positions a line of two or three can share, each slot then left, centre or right.
    private static readonly PitchPosition[] Central =
    [
        PitchPosition.DC, PitchPosition.DM, PitchPosition.MC, PitchPosition.AMC, PitchPosition.ST
    ];

    /// <summary>FM's abbreviation with the side appended, e.g. "DC", "DCL" or "MCR".</summary>
    public string Code => Side switch
    {
        PositionSide.Left => $"{Position}L",
        PositionSide.Right => $"{Position}R",
        _ => Position.ToString()
    };

    /// <summary>Every slot the pitch has, sides included.</summary>
    public static IEnumerable<TacticalPosition> All => Enum.GetValues<PitchPosition>().SelectMany(position =>
        CanHaveSide(position)
            ? new[] { new TacticalPosition(position, PositionSide.Left), new TacticalPosition(position), new TacticalPosition(position, PositionSide.Right) }
            : [new TacticalPosition(position)]);

    public static bool CanHaveSide(PitchPosition position)
    {
        return Central.Contains(position);
    }

    /// <summary>Reads a <see cref="Code"/>: "DL", "AMC", "DCL", "STR". Only central positions take a side.</summary>
    public static bool TryParse(string? code, out TacticalPosition? position)
    {
        position = null;
        var text = code?.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(text) || text.Any(char.IsDigit))
        {
            return false;
        }

        // Whole codes first: DL, ML, AML and WBL end in L without being a side.
        if (Enum.TryParse<PitchPosition>(text, out var whole))
        {
            position = new TacticalPosition(whole);
            return true;
        }

        var side = text[^1] switch
        {
            'L' => PositionSide.Left,
            'R' => PositionSide.Right,
            _ => (PositionSide?)null
        };

        if (side is not null && Enum.TryParse<PitchPosition>(text[..^1], out var central) && CanHaveSide(central))
        {
            position = new TacticalPosition(central, side.Value);
            return true;
        }

        return false;
    }
}
