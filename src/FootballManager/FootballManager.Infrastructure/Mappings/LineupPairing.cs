using System.Numerics;
using FootballManager.Domain.Tactics;
using FootballManager.Infrastructure.Dtos.Matches;

namespace FootballManager.Infrastructure.Mappings;

// Which out-of-possession entry of a past match's lineup is each starter's. The game keeps every player in his own
// slot in both phases, but the out-of-possession positions, with their roles, come in the out-of-possession
// formation's own slot order: when the two formations order their slots differently, a slot holds another player's
// entry (an AMC's and an AML's entries can come back as ML and AMC, positions neither player can play, where the
// live tactic has them AMC and ML). The game keeps no link between the two phases' slots, so each starter is given
// the entry that moves the eleven least over the pitch, his own slot's when that ties.
internal static class LineupPairing
{
    // Above any rounding, below any real difference in distance: on a tie, a starter keeps his own slot's entry.
    private const double MovePenalty = 1e-6;

    // A central slot's side, as a share of the step from the centre to the flank.
    private const double SideOffset = 0.7;

    /// <summary>The out-of-possession entry of each slot's player, by his slot (substitutes keep their own).</summary>
    public static Dictionary<int, BridgeLineupEntryDto> OutOfPossessionBySlot(
        IEnumerable<BridgeLineupEntryDto> withBall, IEnumerable<BridgeLineupEntryDto> withoutBall)
    {
        var withoutBallList = withoutBall.Where(entry => entry.TeamSelectionIndex is not null).ToList();

        var bySlot = withoutBallList
            .GroupBy(entry => entry.TeamSelectionIndex!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        var starters = Starters(withBall);
        var targets = Starters(withoutBallList);

        if (starters.Count == 0 || starters.Count != targets.Count)
        {
            return bySlot;
        }

        var assignment = Assign(starters, targets);

        for (var i = 0; i < starters.Count; i++)
        {
            bySlot[starters[i].Entry.TeamSelectionIndex!.Value] = targets[assignment[i]].Entry;
        }

        return bySlot;
    }

    // The starting eleven's entries with a position, in slot order.
    private static List<(BridgeLineupEntryDto Entry, TacticalPosition Position)> Starters(IEnumerable<BridgeLineupEntryDto> lineup)
    {
        return lineup
            .Where(entry => entry.TeamSelectionIndex is not null && LineupShapes.IsStarter(entry))
            .Select(entry => (Entry: entry, Position: entry.Position is { } mask ? PositionMasks.ToPosition(mask) : null))
            .Where(entry => entry.Position is not null)
            .OrderBy(entry => entry.Entry.TeamSelectionIndex)
            .Select(entry => (entry.Entry, entry.Position!))
            .ToList();
    }

    // The cheapest pairing of starters with entries (eleven at most: every subset of entries is tried, 2^11 states).
    private static int[] Assign(
        List<(BridgeLineupEntryDto Entry, TacticalPosition Position)> starters,
        List<(BridgeLineupEntryDto Entry, TacticalPosition Position)> targets)
    {
        var count = starters.Count;
        var states = 1 << count;
        var best = Enumerable.Repeat(double.MaxValue, states).ToArray();
        var last = new int[states];
        best[0] = 0;

        for (var used = 0; used < states; used++)
        {
            if (best[used] == double.MaxValue)
            {
                continue;
            }

            var starter = BitOperations.PopCount((uint)used);

            if (starter == count)
            {
                continue;
            }

            for (var target = 0; target < count; target++)
            {
                if ((used & (1 << target)) != 0)
                {
                    continue;
                }

                var sameSlot = starters[starter].Entry.TeamSelectionIndex == targets[target].Entry.TeamSelectionIndex;
                var cost = best[used] + Distance(starters[starter].Position, targets[target].Position) + (sameSlot ? 0 : MovePenalty);
                var next = used | (1 << target);

                if (cost < best[next])
                {
                    best[next] = cost;
                    last[next] = target;
                }
            }
        }

        var assignment = new int[count];
        var state = states - 1;

        for (var starter = count - 1; starter >= 0; starter--)
        {
            assignment[starter] = last[state];
            state &= ~(1 << last[state]);
        }

        return assignment;
    }

    // Lines up the pitch from the goalkeeper; columns run from the left flank (-2) to the right (2).
    private static double Distance(TacticalPosition from, TacticalPosition to)
    {
        var (fromRow, fromColumn) = Spot(from);
        var (toRow, toColumn) = Spot(to);
        return Math.Abs(fromRow - toRow) + Math.Abs(fromColumn - toColumn);
    }

    private static (double Row, double Column) Spot(TacticalPosition position)
    {
        var (row, column) = position.Position switch
        {
            PitchPosition.GK => (0, 0),
            PitchPosition.SW => (1, 0),
            PitchPosition.DR => (2, 2),
            PitchPosition.DC => (2, 0),
            PitchPosition.DL => (2, -2),
            PitchPosition.WBR => (3, 2),
            PitchPosition.DM => (3, 0),
            PitchPosition.WBL => (3, -2),
            PitchPosition.MR => (4, 2),
            PitchPosition.MC => (4, 0),
            PitchPosition.ML => (4, -2),
            PitchPosition.AMR => (5, 2),
            PitchPosition.AMC => (5, 0),
            PitchPosition.AML => (5, -2),
            _ => (6, 0) // the striker
        };

        var side = position.Side switch
        {
            PositionSide.Left => -SideOffset,
            PositionSide.Right => SideOffset,
            _ => 0
        };

        return (row, column + side);
    }
}
