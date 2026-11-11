using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.Services;
using FootballManager.Domain.Scouting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class TrialsServiceTests
{
    private readonly IScoutingEditorGameClient _editor = Substitute.For<IScoutingEditorGameClient>();
    private readonly TrialsService _trials;

    public TrialsServiceTests()
    {
        _trials = new TrialsService(_editor);
    }

    [Fact]
    public async Task Applying_needs_the_count_the_user_agreed_to()
    {
        var result = await _trials.OfferAsync([1], weeks: 2, expectedPlayers: null, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().FindPlayersAsync(default!);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public async Task A_trial_lasts_one_to_four_weeks(int weeks)
    {
        Assert.True((await _trials.OfferAsync([1], weeks, null, apply: false)).IsFailed);
    }

    [Fact]
    public async Task The_dry_run_plans_the_length_asked_and_skips_players_by_where_they_stand()
    {
        Players(1, 2, 3);
        Offered(1, PlayerMenuAction.OfferTrial);
        Offered(2, PlayerMenuAction.WithdrawOffer);
        Offered(3, PlayerMenuAction.EndTrial);
        TrialMenu(1);

        var trials = (await _trials.OfferAsync([1, 2, 3], weeks: 3, expectedPlayers: null, apply: false)).Value;

        var planned = Assert.Single(trials.Players);
        Assert.Equal("Offer trial > 3 weeks", planned.Choice);
        Assert.Contains("already waiting", trials.Skipped.Single(skip => skip.PlayerId == 2).Reason);
        Assert.Contains("on trial at the club", trials.Skipped.Single(skip => skip.PlayerId == 3).Reason);
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Nothing_is_sent_when_the_plan_no_longer_has_the_agreed_count()
    {
        Players(1);
        Offered(1, PlayerMenuAction.OfferTrial);
        TrialMenu(1);

        var result = await _trials.OfferAsync([1], weeks: 2, expectedPlayers: 2, apply: true);

        Assert.True(result.IsFailed);
        await _editor.DidNotReceiveWithAnyArgs().RunMenuEntryAsync(default, default, default!);
    }

    [Fact]
    public async Task Applying_sends_the_entry_and_reads_the_offer_back()
    {
        Players(1);
        _editor.GetOfferedActionsAsync(1, Arg.Any<CancellationToken>())
            .Returns(Set(PlayerMenuAction.OfferTrial), Set(PlayerMenuAction.WithdrawOffer));
        TrialMenu(1);

        var trials = (await _trials.OfferAsync([1], weeks: 2, expectedPlayers: 1, apply: true)).Value;

        await _editor.Received(1).RunMenuEntryAsync(1, PlayerMenuAction.OfferTrial,
            Arg.Is<IReadOnlyList<int>>(entry => entry.SequenceEqual(new[] { 0, 1 })), Arg.Any<CancellationToken>());
        Assert.Equal(TrialState.OfferWaiting, trials.Players.Single().Trial);
        Assert.Empty(trials.Notes);
    }

    [Fact]
    public async Task A_game_that_stops_answering_keeps_the_record_of_what_was_sent()
    {
        Players(1, 2);
        _editor.GetOfferedActionsAsync(1, Arg.Any<CancellationToken>())
            .Returns(Set(PlayerMenuAction.OfferTrial), Set(PlayerMenuAction.WithdrawOffer));
        _editor.GetOfferedActionsAsync(2, Arg.Any<CancellationToken>()).Returns(Set(PlayerMenuAction.OfferTrial));
        TrialMenu(1);
        TrialMenu(2);
        _editor.RunMenuEntryAsync(2, PlayerMenuAction.OfferTrial, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new GameUnavailableException("gone"));

        var trials = (await _trials.OfferAsync([1, 2], weeks: 2, expectedPlayers: 2, apply: true)).Value;

        Assert.Equal(1, Assert.Single(trials.Players).PlayerId);
        Assert.Contains("stopped answering", Assert.Single(trials.Notes));
    }

    [Fact]
    public async Task An_ended_trial_still_listed_is_skipped_as_leaving()
    {
        Players(1);
        Offered(1, PlayerMenuAction.EndTrial);
        _editor.GetMenuAsync(1, PlayerMenuAction.EndTrial, Arg.Any<CancellationToken>())
            .Returns(new List<MenuEntry> { new() { Label = "End trial", CanRun = false } });

        var trials = (await _trials.EndAsync([1], expectedPlayers: null, apply: false)).Value;

        Assert.Empty(trials.Players);
        Assert.Contains("already ended", Assert.Single(trials.Skipped).Reason);
    }

    [Fact]
    public async Task Ids_no_list_holds_come_back_as_not_found()
    {
        Players(1);
        Offered(1, PlayerMenuAction.OfferTrial);
        TrialMenu(1);

        var trials = (await _trials.OfferAsync([1, 99], weeks: 1, expectedPlayers: null, apply: false)).Value;

        Assert.Equal([99], trials.NotFound);
    }

    private void Players(params int[] ids)
    {
        _editor.FindPlayersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(ids.ToDictionary(id => id, id => new ScoutedPlayer { Id = id, Name = $"Player {id}" }));
    }

    private void Offered(int playerId, params PlayerMenuAction[] actions)
    {
        _editor.GetOfferedActionsAsync(playerId, Arg.Any<CancellationToken>()).Returns(Set(actions));
    }

    // The game's trial menu: one entry, with the lengths one week to four below it.
    private void TrialMenu(int playerId)
    {
        _editor.GetMenuAsync(playerId, PlayerMenuAction.OfferTrial, Arg.Any<CancellationToken>()).Returns(new List<MenuEntry>
        {
            new()
            {
                Label = "Offer trial",
                Children = Enumerable.Range(1, 4).Select(weeks => new MenuEntry { Label = $"{weeks} weeks", CanRun = true }).ToList()
            }
        });
    }

    private static HashSet<PlayerMenuAction> Set(params PlayerMenuAction[] actions)
    {
        return actions.ToHashSet();
    }
}
