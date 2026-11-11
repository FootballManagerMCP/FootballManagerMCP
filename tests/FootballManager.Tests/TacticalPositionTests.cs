using FootballManager.Domain.Tactics;
using FootballManager.Infrastructure.Mappings;

namespace FootballManager.Tests;

public class TacticalPositionTests
{
    [Theory]
    [InlineData("DCL", PitchPosition.DC, PositionSide.Left)]
    [InlineData("str", PitchPosition.ST, PositionSide.Right)]
    [InlineData("MC", PitchPosition.MC, PositionSide.Centre)]
    // Whole codes that end in L aren't a side.
    [InlineData("DL", PitchPosition.DL, PositionSide.Centre)]
    [InlineData("WBL", PitchPosition.WBL, PositionSide.Centre)]
    [InlineData("AML", PitchPosition.AML, PositionSide.Centre)]
    public void Codes_parse_with_their_side(string code, PitchPosition position, PositionSide side)
    {
        Assert.True(TacticalPosition.TryParse(code, out var parsed));
        Assert.Equal(new TacticalPosition(position, side), parsed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("XX")]
    [InlineData("DR1")]
    // Only central positions take a side.
    [InlineData("GKL")]
    public void Unknown_codes_dont_parse(string code)
    {
        Assert.False(TacticalPosition.TryParse(code, out _));
    }

    [Fact]
    public void Every_slot_round_trips_through_its_code_and_mask()
    {
        foreach (var position in TacticalPosition.All)
        {
            Assert.True(TacticalPosition.TryParse(position.Code, out var parsed));
            Assert.Equal(position, parsed);
            Assert.Equal(position, PositionMasks.ToPosition(PositionMasks.Of(position)));
        }
    }
}
