using FootballManager.Domain.Tactics;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Mappings;

namespace FootballManager.Tests;

public class LineupPairingTests
{
    // A 4-2-3-1: slots 0-10.
    private static readonly string[] WithBall = ["GK", "DR", "DCR", "DCL", "DL", "DMR", "DML", "AMR", "AMC", "AML", "ST"];

    [Fact]
    public void Entries_in_the_same_order_keep_their_slots()
    {
        var paired = LineupPairing.OutOfPossessionBySlot(Lineup(WithBall), Lineup(WithBall));

        for (var slot = 0; slot < WithBall.Length; slot++)
        {
            Assert.Equal(Mask(WithBall[slot]), paired[slot].Position);
        }
    }

    // The game lists out-of-possession entries in that formation's slot order: here the AMC's and AML's come crossed
    // (MC under slot 9, ML under slot 8). Each player gets the entry nearest his own spot.
    [Fact]
    public void Crossed_entries_go_to_the_nearest_player()
    {
        string[] withoutBall = ["GK", "DR", "DCR", "DCL", "DL", "DMR", "DML", "MR", "ML", "MC", "ST"];

        var paired = LineupPairing.OutOfPossessionBySlot(Lineup(WithBall), Lineup(withoutBall));

        Assert.Equal(Mask("MC"), paired[8].Position);
        Assert.Equal(Mask("ML"), paired[9].Position);
        Assert.Equal(Mask("MR"), paired[7].Position);
    }

    [Fact]
    public void Lineups_of_different_sizes_stay_by_slot()
    {
        var withoutBall = Lineup(WithBall).Take(10).ToList();

        var paired = LineupPairing.OutOfPossessionBySlot(Lineup(WithBall), withoutBall);

        Assert.Equal(10, paired.Count);
        Assert.Equal(Mask("AML"), paired[9].Position);
    }

    private static List<BridgeLineupEntryDto> Lineup(IEnumerable<string> codes)
    {
        return codes.Select((code, slot) => new BridgeLineupEntryDto { TeamSelectionIndex = slot, Position = Mask(code) }).ToList();
    }

    private static ulong Mask(string code)
    {
        Assert.True(TacticalPosition.TryParse(code, out var position));
        return PositionMasks.Of(position!);
    }
}
