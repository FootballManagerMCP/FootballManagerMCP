using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Matches;

namespace FootballManager.Infrastructure.Mappings;

// A past match's substitutions, rebuilt from its players' match lines. Each line keeps only the player's last incident
// (came on, went off, booked, injured) with its minute, and the slot he held at the final whistle (-1 once he had left
// the pitch). Nothing says who replaced whom, so players are paired by minute: whoever came on when another went off.
// When several did in the same minute, the slots tell them apart (a substitute finishes in the slot of the player he
// replaced), unless the game moved starters to other slots during the match (a change of shape or a swap): those
// slots then prove nothing, and the minute's pairs are marked uncertain. A substitute who came on and left again, or
// came on and was booked, keeps only his later incident: he's traced to a leaving he could have replaced when he's
// the only one who fits.
internal static class MatchSubstitutions
{
    public static List<SlotChange> Of(IReadOnlyCollection<BridgeLineupEntryDto> lineup, IReadOnlyCollection<BridgePlayerMatchStatsDto> lines)
    {
        var byPlayer = ByPlayer(lines);
        var starters = Starters(lineup);
        var slotOf = starters.ToDictionary(starter => starter.Value, starter => starter.Key);
        var unreliable = MovedSlots(starters, byPlayer);

        var arrivals = Substitutes(lineup, byPlayer, slotOf.Keys.ToHashSet())
            .Select(player => new Arrival(player, byPlayer[player]))
            .ToList();
        var substitutes = arrivals.Select(arrival => arrival.Player).ToHashSet();

        // Everyone who left the pitch without being sent off: starters, and substitutes who came on and left again.
        var leavings = byPlayer
            .Where(player => HasLeft(player.Value) && player.Value.RedCardedInMatch != true)
            .Where(player => slotOf.ContainsKey(player.Key) || substitutes.Contains(player.Key))
            .GroupBy(player => player.Value.MatchTime)
            .OrderBy(group => group.Key ?? int.MaxValue);

        var changes = new List<SlotChange>();

        foreach (var minute in leavings)
        {
            var leaving = minute.Select(player => player.Key).ToList();
            var exact = arrivals.Where(arrival => !arrival.Paired && arrival.MinuteOn is not null && arrival.MinuteOn == minute.Key).ToList();
            var traced = arrivals
                .Where(arrival => !arrival.Paired && arrival.MinuteOn is null && !leaving.Contains(arrival.Player) && arrival.CouldBeOnBy(minute.Key))
                .ToList();

            // Pairs the slots prove: a substitute finishing in the slot the leaver held, where no starter moved.
            foreach (var player in leaving.ToList())
            {
                if (!slotOf.TryGetValue(player, out var slot) || unreliable.Contains(slot))
                {
                    continue;
                }

                var finisher = exact.Concat(traced).Where(arrival => arrival.EndSlot == slot).ToList();

                if (finisher.Count == 1)
                {
                    Pair(changes, slotOf, minute.Key, player, finisher[0], uncertain: false);
                    leaving.Remove(player);
                    exact.Remove(finisher[0]);
                    traced.Remove(finisher[0]);
                }
            }

            // The minute's other arrivals: certain when one player left and one came on.
            leaving = leaving.OrderBy(player => slotOf.GetValueOrDefault(player, int.MaxValue)).ToList();
            exact = exact.OrderBy(arrival => arrival.EndSlot ?? int.MaxValue).ToList();
            var uncertain = leaving.Count > 1 || exact.Count > 1;

            while (leaving.Count > 0 && exact.Count > 0)
            {
                Pair(changes, slotOf, minute.Key, leaving[0], exact[0], uncertain);
                leaving.RemoveAt(0);
                exact.RemoveAt(0);
            }

            // Leavings no one came on for in that minute: substitutes whose minute on is lost, when they're exactly as
            // many. Otherwise who replaced whom can't be told, and the leavings stay unpaired.
            if (leaving.Count > 0 && leaving.Count == traced.Count)
            {
                var several = leaving.Count > 1;

                for (var i = 0; i < leaving.Count; i++)
                {
                    Pair(changes, slotOf, minute.Key, leaving[i], traced[i], several);
                }
            }
        }

        return changes.OrderBy(change => change.Minute ?? int.MaxValue).ThenBy(change => change.Slot ?? int.MaxValue).ToList();
    }

    /// <summary>
    /// True when the game moved some starters to other lineup slots during the match (a change of shape, or players
    /// swapping places): their final slots differ from those they started in.
    /// </summary>
    public static bool Rearranged(IReadOnlyCollection<BridgeLineupEntryDto> lineup, IReadOnlyCollection<BridgePlayerMatchStatsDto> lines)
    {
        return MovedSlots(Starters(lineup), ByPlayer(lines)).Count > 0;
    }

    public static bool CameOn(BridgePlayerMatchStatsDto line)
    {
        return line.IncidentType == MatchConstants.CameOnIncident || line.PlayerTeamSelectionIndex >= 0;
    }

    private static Dictionary<int, BridgePlayerMatchStatsDto> ByPlayer(IReadOnlyCollection<BridgePlayerMatchStatsDto> lines)
    {
        return lines
            .Where(line => line.Player?.Index is not null)
            .GroupBy(line => line.Player!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First());
    }

    // The starting eleven, by slot.
    private static Dictionary<int, int> Starters(IReadOnlyCollection<BridgeLineupEntryDto> lineup)
    {
        return lineup
            .Where(entry => entry.Player?.Index is not null && entry.TeamSelectionIndex is not null && LineupShapes.IsStarter(entry))
            .GroupBy(entry => entry.TeamSelectionIndex!.Value)
            .ToDictionary(group => group.Key, group => group.First().Player!.Index!.Value);
    }

    private static IEnumerable<int> Substitutes(
        IReadOnlyCollection<BridgeLineupEntryDto> lineup, Dictionary<int, BridgePlayerMatchStatsDto> byPlayer, HashSet<int> starters)
    {
        var subbedOn = lineup
            .Where(entry => entry.MatchWasSubbedOn == true && entry.Player?.Index is not null)
            .Select(entry => entry.Player!.Index!.Value)
            .ToHashSet();

        return byPlayer
            .Where(player => !starters.Contains(player.Key) && (subbedOn.Contains(player.Key) || CameOn(player.Value)))
            .OrderBy(player => player.Value.PlayerTeamSelectionIndex is >= 0 ? player.Value.PlayerTeamSelectionIndex : int.MaxValue)
            .Select(player => player.Key);
    }

    // The slots a starter left or moved into while staying on the pitch: whoever finishes in them proves nothing.
    private static HashSet<int> MovedSlots(Dictionary<int, int> starters, Dictionary<int, BridgePlayerMatchStatsDto> byPlayer)
    {
        var moved = new HashSet<int>();

        foreach (var (slot, starter) in starters)
        {
            if (byPlayer.TryGetValue(starter, out var line) && line.PlayerTeamSelectionIndex is { } end && end >= 0 && end != slot)
            {
                moved.Add(slot);
                moved.Add(end);
            }
        }

        return moved;
    }

    private static bool HasLeft(BridgePlayerMatchStatsDto line)
    {
        return line.PlayerTeamSelectionIndex is null or < 0
               && line.IncidentType is MatchConstants.WentOffIncident or MatchConstants.InjuredOffIncident;
    }

    private static void Pair(List<SlotChange> changes, Dictionary<int, int> slotOf, int? minute, int off, Arrival on, bool uncertain)
    {
        int? slot = slotOf.TryGetValue(off, out var held) ? held : null;
        changes.Add(new SlotChange(slot, minute, off, on.Player, uncertain));
        on.Paired = true;

        // A substitute takes the slot of the player he replaced, whatever its number at the end.
        if (slot is { } taken && !uncertain)
        {
            slotOf[on.Player] = taken;
        }
    }

    // A substitute, with when he came on when that's still his last incident; otherwise his later incident (leaving
    // again, or a booking) bounds when he can have come on.
    private sealed class Arrival(int player, BridgePlayerMatchStatsDto line)
    {
        private readonly int? _later = line.IncidentType == MatchConstants.CameOnIncident ? null : line.MatchTime;
        private readonly bool _leftAgain = HasLeft(line);

        public int Player { get; } = player;
        public int? MinuteOn { get; } = line.IncidentType == MatchConstants.CameOnIncident ? line.MatchTime : null;
        public int? EndSlot { get; } = line.PlayerTeamSelectionIndex is >= 0 ? line.PlayerTeamSelectionIndex : null;
        public bool Paired { get; set; }

        // He left again after coming on, or was booked once on.
        public bool CouldBeOnBy(int? minute)
        {
            if (_later is null || minute is null)
            {
                return false;
            }

            return _leftAgain ? _later > minute : _later >= minute;
        }
    }
}
