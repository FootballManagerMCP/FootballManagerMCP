using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Tactics.Editing;
using NSubstitute;

namespace FootballManager.Tests;

public class BenchValidationTests
{
    private const int BenchPlaces = 7;

    private readonly ITacticEditorGameClient _game = Substitute.For<ITacticEditorGameClient>();
    private readonly TacticEditingService _editing;

    public BenchValidationTests()
    {
        _game.GetOptionsAsync(Arg.Any<CancellationToken>()).Returns(new TacticOptions { ActiveSlot = 1 });
        _game.GetTacticAsync(Arg.Any<CancellationToken>()).Returns(_ => Tactic());
        _editing = new TacticEditingService(_game);
    }

    [Fact]
    public async Task A_bench_longer_than_the_competitions_is_refused()
    {
        var result = await _editing.SetTacticAsync(new TacticChanges { Slot = 1, Bench = [.. Enumerable.Range(12, 8)] }, apply: false);

        Assert.Contains($"The bench has {BenchPlaces} places", Messages(result));
    }

    [Fact]
    public async Task A_starter_cant_be_on_the_bench()
    {
        var result = await _editing.SetTacticAsync(new TacticChanges { Slot = 1, Bench = [12, 5] }, apply: false);

        Assert.Contains("starts", Messages(result));
    }

    [Fact]
    public async Task Players_outside_the_squad_are_refused()
    {
        var result = await _editing.SetTacticAsync(new TacticChanges { Slot = 1, Bench = [999] }, apply: false);

        Assert.Contains("No first-team player has id 999", Messages(result));
    }

    [Fact]
    public async Task A_dry_run_plans_each_place_in_order_and_picks_nobody()
    {
        var edit = (await _editing.SetTacticAsync(new TacticChanges { Slot = 1, Bench = [12, 13, 14] }, apply: false)).Value;

        Assert.Equal(["place 1", "place 2", "place 3"], edit.Steps.Select(step => step.Subject));
        Assert.All(edit.Steps, step => Assert.Equal(EditStepStatus.Planned, step.Status));
        await _game.DidNotReceiveWithAnyArgs().PickAsync(default, default);
    }

    [Fact]
    public async Task An_empty_bench_is_flagged_when_none_is_given()
    {
        var edit = (await _editing.SetTacticAsync(new TacticChanges { Slot = 1 }, apply: false)).Value;

        Assert.Contains(edit.Notes, note => note.Contains("The bench is empty"));
    }

    private static string Messages(FluentResults.Result<TacticEdit> result)
    {
        Assert.True(result.IsFailed);
        return string.Join(" ", result.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages));
    }

    // Players 1-11 start, 12-20 don't; the bench's places are 11 to 17, all empty.
    private static EditableTactic Tactic()
    {
        return new EditableTactic
        {
            ActiveSlot = 1,
            Slots = [1, 2, 3],
            Lineup = Enumerable.Range(0, 11).Select(index => new EditableSlot { Index = index, PlayerId = index + 1, PlayerName = $"Player {index + 1}" }).ToList(),
            Bench = Enumerable.Range(11, BenchPlaces).Select(index => new BenchPlace { Index = index }).ToList(),
            Squad = Enumerable.Range(1, 20).ToDictionary(id => id, id => $"Player {id}")
        };
    }
}
