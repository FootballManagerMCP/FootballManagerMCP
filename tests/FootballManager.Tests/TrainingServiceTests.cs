using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Training;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class TrainingServiceTests
{
    private const int Keeper = 1;
    private const int Winger = 2;
    private const int Injured = 3;
    private const string Tuesday = "2026-10-06";
    private const string Monday = "2026-10-05";

    private readonly ITrainingGameClient _game = Substitute.For<ITrainingGameClient>();
    private readonly TrainingService _training;

    public TrainingServiceTests()
    {
        _training = new TrainingService(_game);
        _game.GetTrainingAsync(Arg.Any<CancellationToken>()).Returns(Training());
    }

    [Fact]
    public async Task Nothing_to_change_is_refused_without_asking_the_game()
    {
        var result = await _training.SetTrainingAsync(new TrainingEdits(), apply: false);

        Assert.True(result.IsFailed);
        await _game.DidNotReceiveWithAnyArgs().GetTrainingAsync(default);
    }

    [Fact]
    public async Task A_player_named_twice_is_refused()
    {
        var result = await _training.SetTrainingAsync(Edits(players: [new() { PlayerId = Keeper, Focus = 31 }, new() { PlayerId = Keeper, Intensity = 0 }]), apply: false);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task The_dry_run_plans_from_what_is_set_and_sends_nothing()
    {
        var changes = (await _training.SetTrainingAsync(
            Edits([new() { PlayerId = Keeper, Focus = 31 }], [new() { Date = Tuesday, Slot = 0, Session = 304 }]), apply: false)).Value;

        var keeper = Assert.Single(changes.Players);
        Assert.Equal(32, keeper.Focus!.From!.Id);
        Assert.Equal(31, keeper.Focus.To!.Id);
        Assert.Null(keeper.Focus.Done);
        var session = Assert.Single(changes.Sessions);
        Assert.Equal(303, session.Change.From!.Id);
        Assert.Equal(304, session.Change.To!.Id);
        Assert.False(changes.Applied);
        await _game.DidNotReceiveWithAnyArgs().SetFocusAsync(default, default);
        await _game.DidNotReceiveWithAnyArgs().SetSessionAsync(default!, default, default, default);
    }

    [Fact]
    public async Task A_focus_the_player_is_not_offered_is_refused()
    {
        var result = await _training.SetTrainingAsync(Edits([new() { PlayerId = Winger, Focus = 31 }]), apply: false);

        var error = Assert.IsType<ValidationError>(result.Errors.Single());
        Assert.Contains("can't take focus 31", error.ErrorMessages.Single());
    }

    [Fact]
    public async Task An_injured_player_takes_a_focus_but_not_an_intensity()
    {
        var intensity = await _training.SetTrainingAsync(Edits([new() { PlayerId = Injured, Intensity = 0 }]), apply: false);
        var focus = await _training.SetTrainingAsync(Edits([new() { PlayerId = Injured, Focus = 9 }]), apply: false);

        Assert.True(intensity.IsFailed);
        Assert.Equal(9, focus.Value.Players.Single().Focus!.To!.Id);
    }

    [Theory]
    [InlineData(Monday, 0, 304)]
    [InlineData(Tuesday, 0, 999)]
    [InlineData("2026-12-25", 0, 304)]
    public async Task A_session_the_game_does_not_offer_is_refused(string date, int slot, int session)
    {
        var result = await _training.SetTrainingAsync(Edits(sessions: [new() { Date = date, Slot = slot, Session = session }]), apply: false);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task What_is_already_set_is_left_out_with_a_note()
    {
        var changes = (await _training.SetTrainingAsync(
            Edits([new() { PlayerId = Keeper, Focus = 32 }, new() { PlayerId = Winger, Intensity = TrainingService.AutomaticIntensity }]), apply: false)).Value;

        Assert.Empty(changes.Players);
        Assert.Equal(2, changes.Notes.Count);
    }

    [Fact]
    public async Task Applying_sends_each_change_and_reads_it_back()
    {
        var after = Training();
        after.Players[0].Focus = new TrainingOption { Id = 31, Name = "Couverture GB" };
        after.Weeks[0].Days[1].Sessions[0].Type = 304;
        _game.GetTrainingAsync(Arg.Any<CancellationToken>()).Returns(Training(), after);

        var changes = (await _training.SetTrainingAsync(
            Edits([new() { PlayerId = Keeper, Focus = 31 }], [new() { Date = Tuesday, Slot = 0, Session = 304 }]), apply: true)).Value;

        await _game.Received(1).SetFocusAsync(Keeper, 31, Arg.Any<CancellationToken>());
        await _game.Received(1).SetSessionAsync(Tuesday, 0, 303, 304, Arg.Any<CancellationToken>());
        Assert.True(changes.Players.Single().Focus!.Done);
        Assert.True(changes.Sessions.Single().Change.Done);
        Assert.Empty(changes.Notes);
    }

    [Fact]
    public async Task A_rest_is_done_once_the_game_shows_him_resting()
    {
        // Asked for 3 days, the game shows the days left: 4, counting today.
        var after = Training();
        after.Players[0].Intensity = new TrainingOption { Id = 13, Name = "4 jours" };
        after.Players[0].RestDays = 4;
        _game.GetTrainingAsync(Arg.Any<CancellationToken>()).Returns(Training(), after);

        var changes = (await _training.SetTrainingAsync(Edits([new() { PlayerId = Keeper, Intensity = 12 }]), apply: true)).Value;

        await _game.Received(1).SetIntensityAsync(Keeper, 12, Arg.Any<CancellationToken>());
        Assert.True(changes.Players.Single().Intensity!.Done);
    }

    [Fact]
    public async Task The_first_change_the_game_refuses_stops_the_rest()
    {
        _game.SetFocusAsync(Keeper, 31, Arg.Any<CancellationToken>()).ThrowsAsync(new GameRejectedException("no"));

        var changes = (await _training.SetTrainingAsync(
            Edits([new() { PlayerId = Keeper, Focus = 31 }], [new() { Date = Tuesday, Slot = 0, Session = 304 }]), apply: true)).Value;

        await _game.DidNotReceiveWithAnyArgs().SetSessionAsync(default!, default, default, default);
        Assert.False(changes.Players.Single().Focus!.Done);
        Assert.False(changes.Sessions.Single().Change.Done);
        Assert.Contains(changes.Notes, note => note.Contains("didn't take"));
    }

    private static TrainingEdits Edits(List<PlayerTrainingEdit>? players = null, List<SessionEdit>? sessions = null)
    {
        return new TrainingEdits { Players = players ?? [], Sessions = sessions ?? [] };
    }

    private static TrainingOverview Training()
    {
        return new TrainingOverview
        {
            Weeks =
            [
                new TrainingWeek
                {
                    StartDate = Monday,
                    Days =
                    [
                        new TrainingDay { Date = Monday, Sessions = [new TrainingSession { Slot = 0, Type = 600, Name = "Pas disponible", Category = -1 }] },
                        new TrainingDay
                        {
                            Date = Tuesday,
                            Sessions =
                            [
                                new TrainingSession { Slot = 0, Type = 303, Name = "Condition physique", Category = 3, CanEdit = true },
                                new TrainingSession { Slot = 1, Type = 200, Name = "Développement individuel", Category = 2, CanEdit = true }
                            ]
                        }
                    ]
                }
            ],
            SessionTypes =
            [
                new TrainingSessionCategory
                {
                    Id = 3,
                    Name = "Physique et récupération",
                    Sessions = [Option(303, "Condition physique"), Option(304, "Condition physique intensive")]
                }
            ],
            FocusOptions = [Option(-1, "---"), Option(9, "Vivacité"), Option(23, "Passes"), Option(31, "Couverture GB"), Option(32, "Relances (GB)")],
            IntensityOptions = [Option(-1, "Automatique"), Option(0, "Semi-intensité"), Option(1, "Intensité normale"), Option(12, "3 jours")],
            Players =
            [
                new PlayerTraining
                {
                    PlayerId = Keeper, Name = "Joe Massaro", CanEditFocus = true, CanEditIntensity = true, Focus = Option(32, "Relances (GB)"),
                    Intensity = Option(1, "Intensité normale"), FocusChoices = [-1, 9, 31, 32], IntensityChoices = [-1, 0, 1, 12]
                },
                new PlayerTraining
                {
                    PlayerId = Winger, Name = "Osian Dillon", CanEditFocus = true, CanEditIntensity = true, Intensity = Option(1, "Intensité normale"),
                    IntensityIsAutomatic = true, FocusChoices = [-1, 9, 23], IntensityChoices = [-1, 0, 1, 12]
                },
                new PlayerTraining { PlayerId = Injured, Name = "Adam Smith", CanEditFocus = true, FocusChoices = [-1, 9, 23], IntensityChoices = [-1, 0, 1, 12] }
            ]
        };
    }

    private static TrainingOption Option(long id, string name)
    {
        return new TrainingOption { Id = id, Name = name };
    }
}
