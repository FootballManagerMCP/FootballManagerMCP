using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Mappings;

namespace FootballManager.Tests;

public class MatchSubstitutionsTests
{
    private const int BookedIncident = 3;
    private const int OffThePitch = -1;

    [Fact]
    public void A_substitute_replaces_the_starter_of_the_slot_he_finishes_in()
    {
        var lineup = Starters(11);
        List<BridgePlayerMatchStatsDto> lines =
        [
            Line(player: 109, incident: MatchConstants.WentOffIncident, minute: 60, slot: OffThePitch),
            Line(player: 200, incident: MatchConstants.CameOnIncident, minute: 60, slot: 9)
        ];

        var change = Assert.Single(MatchSubstitutions.Of(lineup, lines));

        Assert.Equal(new SlotChange(9, 60, 109, 200), change);
    }

    [Fact]
    public void A_match_played_through_has_no_substitutions()
    {
        Assert.Empty(MatchSubstitutions.Of(Starters(11), [Line(player: 104, incident: BookedIncident, minute: 30, slot: 4)]));
    }

    [Fact]
    public void A_substitute_who_came_on_and_went_off_again_is_traced_between_the_two()
    {
        // His last incident is going off again: only the lineup says he came on.
        var lineup = Starters(11);
        lineup.Add(Substitute(200, selectionIndex: 13));
        List<BridgePlayerMatchStatsDto> lines =
        [
            Line(player: 110, incident: MatchConstants.WentOffIncident, minute: 46, slot: OffThePitch),
            Line(player: 200, incident: MatchConstants.WentOffIncident, minute: 70, slot: OffThePitch),
            Line(player: 201, incident: MatchConstants.CameOnIncident, minute: 70, slot: 10)
        ];

        var changes = MatchSubstitutions.Of(lineup, lines);

        Assert.Equal([new SlotChange(10, 46, 110, 200), new SlotChange(10, 70, 200, 201)], changes);
    }

    [Fact]
    public void Changes_in_the_same_minute_are_told_apart_by_their_slots()
    {
        List<BridgePlayerMatchStatsDto> lines =
        [
            Line(player: 103, incident: MatchConstants.WentOffIncident, minute: 70, slot: OffThePitch),
            Line(player: 108, incident: MatchConstants.WentOffIncident, minute: 70, slot: OffThePitch),
            Line(player: 200, incident: MatchConstants.CameOnIncident, minute: 70, slot: 8),
            Line(player: 201, incident: MatchConstants.CameOnIncident, minute: 70, slot: 3)
        ];

        var changes = MatchSubstitutions.Of(Starters(11), lines);

        Assert.Equal([new SlotChange(3, 70, 103, 201), new SlotChange(8, 70, 108, 200)], changes);
    }

    [Fact]
    public void When_the_game_renumbers_the_slots_changes_in_the_same_minute_are_uncertain()
    {
        // As at Baglan: starters moved to other slots during the match, so the substitutes' final slots prove nothing.
        var lineup = Starters(11);
        lineup.Add(Substitute(200, selectionIndex: 14));
        List<BridgePlayerMatchStatsDto> lines =
        [
            Line(player: 101, incident: 0, minute: 0, slot: 9),
            Line(player: 103, incident: 0, minute: 0, slot: 2),
            Line(player: 107, incident: 0, minute: 0, slot: 10),
            Line(player: 110, incident: MatchConstants.WentOffIncident, minute: 5, slot: OffThePitch),
            Line(player: 105, incident: MatchConstants.WentOffIncident, minute: 71, slot: OffThePitch),
            Line(player: 200, incident: MatchConstants.InjuredOffIncident, minute: 71, slot: OffThePitch),
            Line(player: 201, incident: MatchConstants.CameOnIncident, minute: 71, slot: 6),
            Line(player: 202, incident: MatchConstants.CameOnIncident, minute: 71, slot: 8)
        ];

        var changes = MatchSubstitutions.Of(lineup, lines);

        // The early change is the only one that fits: certain. The two at 71' can't be told apart.
        Assert.Equal(new SlotChange(10, 5, 110, 200), changes[0]);
        Assert.Equal(3, changes.Count);
        Assert.All(changes.Skip(1), change => Assert.True(change.Uncertain));
        Assert.Equal([105, 200], changes.Skip(1).Select(change => change.OffPlayer).Order());
        Assert.Equal([201, 202], changes.Skip(1).Select(change => change.OnPlayer).Order());
        Assert.True(MatchSubstitutions.Rearranged(lineup, lines));
    }

    [Fact]
    public void One_change_in_a_minute_is_certain_even_when_the_slots_were_renumbered()
    {
        List<BridgePlayerMatchStatsDto> lines =
        [
            Line(player: 101, incident: 0, minute: 0, slot: 9),
            Line(player: 109, incident: 0, minute: 0, slot: 1),
            Line(player: 104, incident: MatchConstants.WentOffIncident, minute: 80, slot: OffThePitch),
            Line(player: 200, incident: MatchConstants.CameOnIncident, minute: 80, slot: 7)
        ];

        var change = Assert.Single(MatchSubstitutions.Of(Starters(11), lines));

        Assert.Equal(new SlotChange(4, 80, 104, 200), change);
    }

    [Fact]
    public void A_player_sent_off_is_no_substitution()
    {
        var sentOff = Line(player: 104, incident: BookedIncident, minute: 43, slot: OffThePitch);
        sentOff.RedCardedInMatch = true;
        List<BridgePlayerMatchStatsDto> lines =
        [
            sentOff,
            Line(player: 109, incident: MatchConstants.WentOffIncident, minute: 72, slot: OffThePitch),
            Line(player: 200, incident: MatchConstants.CameOnIncident, minute: 72, slot: 9)
        ];

        var change = Assert.Single(MatchSubstitutions.Of(Starters(11), lines));

        Assert.Equal(new SlotChange(9, 72, 109, 200), change);
    }

    [Fact]
    public void Starters_who_keep_their_slots_leave_the_lineup_as_it_was()
    {
        List<BridgePlayerMatchStatsDto> lines =
        [
            Line(player: 101, incident: 0, minute: 0, slot: 1),
            Line(player: 109, incident: MatchConstants.WentOffIncident, minute: 60, slot: OffThePitch),
            Line(player: 200, incident: MatchConstants.CameOnIncident, minute: 60, slot: 9)
        ];

        Assert.False(MatchSubstitutions.Rearranged(Starters(11), lines));
    }

    // Players 100-110 in slots 0-10.
    private static List<BridgeLineupEntryDto> Starters(int count)
    {
        return Enumerable.Range(0, count)
            .Select(slot => new BridgeLineupEntryDto { TeamSelectionIndex = slot, Player = new BridgeReferenceDto { Index = 100 + slot } })
            .ToList();
    }

    private static BridgeLineupEntryDto Substitute(int player, int selectionIndex)
    {
        return new BridgeLineupEntryDto { TeamSelectionIndex = selectionIndex, MatchWasSubbedOn = true, Player = new BridgeReferenceDto { Index = player } };
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
