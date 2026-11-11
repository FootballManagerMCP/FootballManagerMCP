using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Services;

// Reading the menus the game builds on a player: an entry by its path (an index per level, from the top), its label,
// the confirmation the game asks for, and the reading of many players' menus at once.
internal static class PlayerMenus
{
    private const string Separator = " > ";

    // How many players' menus are read at once: 28 take 6 seconds instead of 21 one after another.
    public const int MenusAtOnce = 4;

    public static MenuEntry Entry(List<MenuEntry> menu, IReadOnlyList<int> path)
    {
        var entry = menu[path[0]];

        foreach (var index in path.Skip(1))
        {
            entry = entry.Children[index];
        }

        return entry;
    }

    // "Assign scout > [a scout's name] (Scout) > 1 week", as the game labels each level.
    public static string Label(List<MenuEntry> menu, IReadOnlyList<int> path)
    {
        var labels = new List<string>();
        var entries = menu;

        foreach (var index in path)
        {
            labels.Add(entries[index].Label);
            entries = entries[index].Children;
        }

        return string.Join(Separator, labels);
    }

    // The game's confirmation for the entry at path. An entry under a named person sits deeper than the plugin expands
    // its payload; the confirmation (the costs, the scouting network) is the player's rather than the scout's, so it's
    // the one of the same duration under another entry, such as any scout.
    public static (string? Text, bool IsWarning) ConfirmationOf(List<MenuEntry> menu, List<int> path)
    {
        var entry = Entry(menu, path);

        if (entry.Confirmation is not null || path.Count < 3)
        {
            return (entry.Confirmation, entry.ConfirmationIsWarning);
        }

        var same = menu
            .Where(top => top.Children.Count > path[^1])
            .Select(top => top.Children[path[^1]])
            .FirstOrDefault(child => child.Confirmation is not null);

        return same is null ? (null, false) : (same.Confirmation, same.ConfirmationIsWarning);
    }

    public static SkippedPlayer Skip(ScoutedPlayer player, string reason)
    {
        return new SkippedPlayer { PlayerId = player.Id, PlayerName = player.Name, Reason = reason };
    }

    // Runs work on every item, a few at a time: one after another, a squad's menus take twenty seconds, and all at once
    // they'd crowd the plugin. Results come back in the items' order.
    public static async Task<TResult[]> AFewAtATimeAsync<T, TResult>(
        IEnumerable<T> items, int atOnce, Func<T, Task<TResult>> work, CancellationToken cancellationToken)
    {
        using var gate = new SemaphoreSlim(atOnce);

        return await Task.WhenAll(items.Select(async item =>
        {
            await gate.WaitAsync(cancellationToken);

            try
            {
                return await work(item);
            }
            finally
            {
                gate.Release();
            }
        }));
    }
}
