using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Matches;

namespace FootballManager.Infrastructure.Mappings;

// A past match's substitutions, rebuilt from its players' match lines. Each line keeps only the player's last incident
// (came on, went off, booked, injured) with its minute, and the slot he held at the final whistle (-1 once he had left
// the pitch). A substitute takes the slot of the player he replaces, and starters keep theirs, so a slot's finisher
// replaced its starter. A substitute who came on and left again keeps only his leaving: he's traced as the slot's
// holder in between when he's the only one who fits, and left out otherwise.
internal static class MatchSubstitutions
{
    public static List<SlotChange> Of(IReadOnlyCollection<BridgeLineupEntryDto> lineup, IReadOnlyCollection<BridgePlayerMatchStatsDto> lines)
    {
        var byPlayer = lines
            .Where(line => line.Player?.Index is not null)
            .GroupBy(line => line.Player!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        var starters = lineup
            .Where(entry => entry.Player?.Index is not null && entry.TeamSelectionIndex is not null && LineupShapes.IsStarter(entry))
            .GroupBy(entry => entry.TeamSelectionIndex!.Value)
            .ToDictionary(group => group.Key, group => group.First().Player!.Index!.Value);

        var startingPlayers = starters.Values.ToHashSet();
        var subbedOn = lineup
            .Where(entry => entry.MatchWasSubbedOn == true && entry.Player?.Index is not null)
            .Select(entry => entry.Player!.Index!.Value)
            .ToHashSet();

        var substitutes = byPlayer
            .Where(player => !startingPlayers.Contains(player.Key) && (subbedOn.Contains(player.Key) || CameOn(player.Value)))
            .ToList();

        // On the pitch at the end, by slot, with when they came on if that's still their last incident.
        var finishers = substitutes
            .Where(player => player.Value.PlayerTeamSelectionIndex >= 0)
            .GroupBy(player => player.Value.PlayerTeamSelectionIndex!.Value)
            .ToDictionary(group => group.Key, group => (
                Player: group.First().Key,
                Arrived: group.First().Value.IncidentType == MatchConstants.CameOnIncident ? group.First().Value.MatchTime : null));

        var leftAgain = substitutes
            .Where(player => player.Value.PlayerTeamSelectionIndex is null or < 0)
            .Select(player => (Player: player.Key, Left: player.Value.MatchTime))
            .ToList();

        var changes = new List<SlotChange>();
        var unfilled = new List<(int Slot, int Starter, int? Left)>();

        foreach (var (slot, starter) in starters)
        {
            // Played to the end.
            if (!byPlayer.TryGetValue(starter, out var line) || line.PlayerTeamSelectionIndex is null or >= 0)
            {
                continue;
            }

            var left = line.MatchTime;

            if (!finishers.TryGetValue(slot, out var finisher))
            {
                unfilled.Add((slot, starter, left));
                continue;
            }

            if (finisher.Arrived is null || finisher.Arrived == left)
            {
                changes.Add(new SlotChange(slot, left, starter, finisher.Player));
                continue;
            }

            // Someone held the slot in between: the one substitute who left as the finisher came on.
            var between = leftAgain.Where(player => player.Left == finisher.Arrived).ToList();

            if (between.Count == 1)
            {
                changes.Add(new SlotChange(slot, left, starter, between[0].Player));
                changes.Add(new SlotChange(slot, finisher.Arrived, between[0].Player, finisher.Player));
                leftAgain.Remove(between[0]);
            }
        }

        // A slot left empty (a red card, or an injury with no substitutes left) by the one substitute who left after
        // its starter.
        if (unfilled.Count == 1 && leftAgain.Count == 1 && leftAgain[0].Left >= unfilled[0].Left)
        {
            changes.Add(new SlotChange(unfilled[0].Slot, unfilled[0].Left, unfilled[0].Starter, leftAgain[0].Player));
        }

        return changes.OrderBy(change => change.Minute ?? int.MaxValue).ThenBy(change => change.Slot).ToList();
    }

    public static bool CameOn(BridgePlayerMatchStatsDto line)
    {
        return line.IncidentType == MatchConstants.CameOnIncident || line.PlayerTeamSelectionIndex >= 0;
    }
}
