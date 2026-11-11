using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Mappings;

namespace FootballManager.Tests;

public class MatchSubstitutionsTests
{
    private const int WentOffIncident = 2;
    private const int OffThePitch = -1;

    [Fact]
    public void A_substitute_replaces_the_starter_of_the_slot_he_finishes_in()
    {
        var lineup = Starters(11);
        List<BridgePlayerMatchStatsDto> lines =
        [
            Line(player: 109, incident: WentOffIncident, minute: 60, slot: OffThePitch),
            Line(player: 200, incident: MatchConstants.CameOnIncident, minute: 60, slot: 9)
        ];

        var change = Assert.Single(MatchSubstitutions.Of(lineup, lines));

        Assert.Equal(new SlotChange(9, 60, 109, 200), change);
    }

    [Fact]
    public void A_match_played_through_has_no_substitutions()
    {
        Assert.Empty(MatchSubstitutions.Of(Starters(11), [Line(player: 104, incident: 3, minute: 30, slot: 4)]));
    }

    [Fact]
    public void A_substitute_who_came_on_and_went_off_again_is_traced_between_the_two()
    {
        // His last incident is going off again: only the lineup says he came on.
        var lineup = Starters(11);
        lineup.Add(new BridgeLineupEntryDto { TeamSelectionIndex = 13, MatchWasSubbedOn = true, Player = new BridgeReferenceDto { Index = 200 } });
        List<BridgePlayerMatchStatsDto> lines =
        [
            Line(player: 110, incident: WentOffIncident, minute: 46, slot: OffThePitch),
            Line(player: 200, incident: WentOffIncident, minute: 70, slot: OffThePitch),
            Line(player: 201, incident: MatchConstants.CameOnIncident, minute: 70, slot: 10)
        ];

        var changes = MatchSubstitutions.Of(lineup, lines);

        Assert.Equal([new SlotChange(10, 46, 110, 200), new SlotChange(10, 70, 200, 201)], changes);
    }

    // Players 100-110 in slots 0-10.
    private static List<BridgeLineupEntryDto> Starters(int count)
    {
        return Enumerable.Range(0, count)
            .Select(slot => new BridgeLineupEntryDto { TeamSelectionIndex = slot, Player = new BridgeReferenceDto { Index = 100 + slot } })
            .ToList();
    }

    private static BridgePlayerMatchStatsDto Line(int player, int incident, int minute, int slot)
    {
        return new BridgePlayerMatchStatsDto
        {
            Player = new BridgeReferenceDto { Index = player },
            IncidentType = incident,
            MatchTime = minute,
            PlayerTeamSelectionIndex = slot
        };
    }
}
