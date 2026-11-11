using FootballManager.Application.Services;
using FootballManager.Domain.Scouting;

namespace FootballManager.Tests;

public class PlayerMenusTests
{
    // Any scout, with its durations; then a named scout, whose durations the plugin expands without their payload.
    private static readonly List<MenuEntry> ScoutMenu =
    [
        new()
        {
            Label = "Any scout",
            Children = [Duration("1 week", "Costs to expect"), Duration("2 weeks", "Costs to expect")]
        },
        new()
        {
            Label = "Named scouts",
            Children = [new MenuEntry { Label = "A Scout (Scout)", Children = [Duration("1 week", null), Duration("2 weeks", null)] }]
        }
    ];

    [Fact]
    public void An_entry_is_labelled_level_by_level()
    {
        Assert.Equal("Named scouts > A Scout (Scout) > 2 weeks", PlayerMenus.Label(ScoutMenu, [1, 0, 1]));
    }

    [Fact]
    public void A_named_scouts_confirmation_is_the_same_durations_under_any_scout()
    {
        Assert.Equal("Costs to expect", PlayerMenus.ConfirmationOf(ScoutMenu, [1, 0, 1]).Text);
    }

    [Fact]
    public async Task A_few_at_a_time_keeps_the_items_order()
    {
        var results = await PlayerMenus.AFewAtATimeAsync(Enumerable.Range(1, 10), 3, async item =>
        {
            await Task.Delay(10 - item);
            return item * 2;
        }, CancellationToken.None);

        Assert.Equal(Enumerable.Range(1, 10).Select(item => item * 2), results);
    }

    private static MenuEntry Duration(string label, string? confirmation)
    {
        return new MenuEntry { Label = label, CanRun = true, Confirmation = confirmation };
    }
}
