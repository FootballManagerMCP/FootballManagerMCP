using System.Numerics;
using FootballManager.Domain.Tactics;

namespace FootballManager.Infrastructure.Mappings;

// The game sends a tactics position as a bit mask: bits 0-14 are the positions in FM's classic order, bit 20 marks the
// left slot of a central pair or trio and bit 21 the right one.
internal static class PositionMasks
{
    private const ulong PositionBits = (1UL << 15) - 1;
    private const ulong LeftOfCentre = 1UL << 20;
    private const ulong RightOfCentre = 1UL << 21;

    private static readonly PitchPosition[] ByBit =
    [
        PitchPosition.GK, PitchPosition.SW, PitchPosition.DR, PitchPosition.DL, PitchPosition.DC, PitchPosition.WBR,
        PitchPosition.WBL, PitchPosition.DM, PitchPosition.MR, PitchPosition.ML, PitchPosition.MC, PitchPosition.AMR,
        PitchPosition.AML, PitchPosition.AMC, PitchPosition.ST
    ];

    public static ulong Of(PitchPosition position)
    {
        return 1UL << Array.IndexOf(ByBit, position);
    }

    /// <summary>The mask of a slot, side included, as the game moves a slot to it.</summary>
    public static ulong Of(TacticalPosition position)
    {
        return Of(position.Position) | position.Side switch
        {
            PositionSide.Left => LeftOfCentre,
            PositionSide.Right => RightOfCentre,
            _ => 0
        };
    }

    /// <summary>The position alone, without the side bits (what role tables are keyed by).</summary>
    public static ulong WithoutSide(ulong mask)
    {
        return mask & PositionBits;
    }

    public static TacticalPosition? ToPosition(ulong mask)
    {
        var bits = WithoutSide(mask);

        if (bits == 0 || !BitOperations.IsPow2(bits))
        {
            return null;
        }

        var side = (mask & LeftOfCentre) != 0 ? PositionSide.Left
            : (mask & RightOfCentre) != 0 ? PositionSide.Right
            : PositionSide.Centre;

        return new TacticalPosition(ByBit[BitOperations.TrailingZeroCount(bits)], side);
    }
}
