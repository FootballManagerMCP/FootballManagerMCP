using FootballManager.Application.Interfaces;
using FootballManager.Application.Services;
using FootballManager.Domain.Scouting;
using NSubstitute;

namespace FootballManager.Tests;

public class ScoutTeamTests
{
    private readonly IScoutingEditorGameClient _editor = Substitute.For<IScoutingEditorGameClient>();
    private readonly IScoutingGameClient _scouting = Substitute.For<IScoutingGameClient>();
    private readonly ScoutingEditingService _editing;

    public ScoutTeamTests()
    {
        _editing = new ScoutingEditingService(_editor, _scouting);
    }

    [Fact]
    public async Task Applying_needs_the_count_the_user_agreed_to()
    {
        var result = await _editing.ScoutTeamAsync(1, null, ScoutingDuration.OneWeek, expectedPlayers: null, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().GetTeamPlayersAsync(default);
    }

    [Fact]
    public async Task Nothing_is_sent_when_the_squad_no_longer_matches_the_agreed_count()
    {
        _editor.GetTeamPlayersAsync(1, Arg.Any<CancellationToken>()).Returns(new TeamPlayers
        {
            TeamId = 1,
            TeamName = "A team",
            Players = [new ScoutedPlayer { Id = 10, Name = "One" }, new ScoutedPlayer { Id = 11, Name = "Two" }]
        });
        _editor.GetMenuAsync(Arg.Any<int>(), PlayerMenuAction.AssignScout, Arg.Any<CancellationToken>()).Returns(AnyScoutMenu());

        var result = await _editing.ScoutTeamAsync(1, null, ScoutingDuration.OneWeek, expectedPlayers: 3, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Players_already_scouted_are_skipped()
    {
        _editor.GetTeamPlayersAsync(1, Arg.Any<CancellationToken>()).Returns(new TeamPlayers
        {
            TeamId = 1,
            TeamName = "A team",
            Players = [new ScoutedPlayer { Id = 10, Name = "One", IsBeingScouted = true }, new ScoutedPlayer { Id = 11, Name = "Two" }]
        });
        _editor.GetMenuAsync(Arg.Any<int>(), PlayerMenuAction.AssignScout, Arg.Any<CancellationToken>()).Returns(AnyScoutMenu());

        var change = (await _editing.ScoutTeamAsync(1, null, ScoutingDuration.OneWeek, expectedPlayers: null, apply: false)).Value;

        Assert.Equal(11, Assert.Single(change.Players).PlayerId);
        Assert.Equal(10, Assert.Single(change.Skipped).PlayerId);
    }

    // Any scout, with the ten durations in the game's order.
    private static List<MenuEntry>? AnyScoutMenu()
    {
        return
        [
            new MenuEntry
            {
                Label = "Any scout",
                Children = Enum.GetValues<ScoutingDuration>().Select(duration => new MenuEntry { Label = duration.ToString(), CanRun = true }).ToList()
            }
        ];
    }
}
