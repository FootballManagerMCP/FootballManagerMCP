using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Scouting;
using NSubstitute;

namespace FootballManager.Tests;

public class ShortlistTests
{
    private const int Player = 29237533;
    private static readonly Shortlist Default = new() { Name = "(Par défaut)", Players = 14, IsDefault = true };
    private static readonly Shortlist Moneyball = new() { Name = "Moneyball", Players = 1, IsSelected = true };
    private static readonly string[] Durations = ["Indéfiniment", "1 mois", "3 mois", "6 mois", "1 an"];

    private readonly IScoutingEditorGameClient _editor = Substitute.For<IScoutingEditorGameClient>();
    private readonly IScoutingGameClient _scouting = Substitute.For<IScoutingGameClient>();
    private readonly ScoutingEditingService _editing;

    public ShortlistTests()
    {
        _editing = new ScoutingEditingService(_editor, _scouting);
        _scouting.GetShortlistsAsync(Arg.Any<CancellationToken>()).Returns([Default, Moneyball]);
        _editor.FindPlayerAsync(Player, Arg.Any<CancellationToken>()).Returns(new ScoutedPlayer { Id = Player, Name = "Joe Woodiwiss" });
    }

    [Fact]
    public void With_one_shortlist_the_duration_comes_straight_under_the_entry()
    {
        var path = ScoutingEditingService.PickShortlist(OneListMenu(), [Default], null, ShortlistDuration.ThreeMonths);

        Assert.Equal([0, 2], path.Value);
    }

    [Fact]
    public void With_one_shortlist_naming_it_still_picks_its_duration()
    {
        var path = ScoutingEditingService.PickShortlist(OneListMenu(), [Default], "(par defaut)", ShortlistDuration.Indefinitely);

        Assert.Equal([0, 0], path.Value);
    }

    [Fact]
    public void With_one_shortlist_another_name_is_refused()
    {
        var path = ScoutingEditingService.PickShortlist(OneListMenu(), [Default], "Moneyball", ShortlistDuration.Indefinitely);

        Assert.True(path.IsFailed);
    }

    [Fact]
    public void With_several_shortlists_the_named_one_is_picked_then_its_duration()
    {
        var path = ScoutingEditingService.PickShortlist(SeveralListsMenu(), [Default, Moneyball], "moneyball", ShortlistDuration.OneYear);

        Assert.Equal([0, 0, 4], path.Value);
    }

    [Fact]
    public void With_several_shortlists_one_must_be_named()
    {
        var path = ScoutingEditingService.PickShortlist(SeveralListsMenu(), [Default, Moneyball], null, ShortlistDuration.Indefinitely);

        var message = Assert.IsType<ValidationError>(path.Errors[0]).ErrorMessages.Single();
        Assert.Contains("'Moneyball'", message);
        Assert.Contains("'(Par défaut)'", message);
    }

    [Fact]
    public void An_unknown_shortlist_lists_those_the_game_offers()
    {
        var path = ScoutingEditingService.PickShortlist(SeveralListsMenu(), [Default, Moneyball], "Targets", ShortlistDuration.Indefinitely);

        Assert.Contains("'Moneyball'", Assert.IsType<ValidationError>(path.Errors[0]).ErrorMessages.Single());
    }

    [Fact]
    public async Task The_dry_run_names_the_list_and_duration_without_sending()
    {
        _editor.GetMenuAsync(Player, PlayerMenuAction.AddToShortlist, Arg.Any<CancellationToken>()).Returns(SeveralListsMenu());

        var change = (await _editing.ShortlistPlayerAsync(Player, "Moneyball", ShortlistDuration.Indefinitely, apply: false)).Value;

        Assert.Equal("Ajouter à la sélection > Moneyball > Indéfiniment", change.Choice);
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Applying_runs_the_picked_entry()
    {
        _editor.GetMenuAsync(Player, PlayerMenuAction.AddToShortlist, Arg.Any<CancellationToken>()).Returns(SeveralListsMenu());

        await _editing.ShortlistPlayerAsync(Player, "(Par défaut)", ShortlistDuration.SixMonths, apply: true);

        await _editor.Received().RunMenuEntryAsync(
            Player, PlayerMenuAction.AddToShortlist, Arg.Is<IReadOnlyList<int>>(path => path.SequenceEqual(new[] { 0, 1, 3 })), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task A_shortlist_needs_a_name(string name)
    {
        var result = await _editing.CreateShortlistAsync(name, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().CreateShortlistAsync(default!);
    }

    [Fact]
    public async Task A_name_already_taken_is_refused_whatever_its_case()
    {
        var result = await _editing.CreateShortlistAsync("MONEYBALL", apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().CreateShortlistAsync(default!);
    }

    [Fact]
    public async Task The_dry_run_checks_the_name_and_lists_the_shortlists()
    {
        var creation = (await _editing.CreateShortlistAsync(" Targets ", apply: false)).Value;

        Assert.Equal("Targets", creation.Name);
        Assert.False(creation.Applied);
        Assert.Null(creation.Created);
        Assert.Equal(2, creation.Shortlists.Count);
        await _editor.DidNotReceiveWithAnyArgs().CreateShortlistAsync(default!);
    }

    [Fact]
    public async Task Applying_makes_the_shortlist_with_the_trimmed_name()
    {
        var creation = (await _editing.CreateShortlistAsync(" Targets ", apply: true)).Value;

        await _editor.Received().CreateShortlistAsync("Targets", Arg.Any<CancellationToken>());
        Assert.True(creation.Created);
    }

    // As the game builds the menu with only the default shortlist: its durations straight under the entry.
    private static List<MenuEntry> OneListMenu()
    {
        return [new MenuEntry { Label = "Ajouter à la sélection", Children = DurationEntries() }];
    }

    // With several, the entry lists them, each with its durations.
    private static List<MenuEntry> SeveralListsMenu()
    {
        return
        [
            new MenuEntry
            {
                Label = "Ajouter à la sélection",
                Children =
                [
                    new MenuEntry { Label = "Moneyball", Children = DurationEntries() },
                    new MenuEntry { Label = "(Par défaut)", Children = DurationEntries() }
                ]
            }
        ];
    }

    private static List<MenuEntry> DurationEntries()
    {
        return Durations.Select(label => new MenuEntry { Label = label, CanRun = true }).ToList();
    }
}
