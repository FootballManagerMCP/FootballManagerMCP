using FootballManager.Domain.Tactics;
using FootballManager.Infrastructure.Dtos.Matches;

namespace FootballManager.Infrastructure.Mappings;

// The shape a team lined up in, from a past match's lineup in one phase: its starting eleven's positions.
internal static class LineupShapes
{
    private const int StartingEleven = 11;

    public static bool IsStarter(BridgeLineupEntryDto entry)
    {
        return entry.MatchWasSubbedOn != true && entry.TeamSelectionIndex < StartingEleven;
    }

    public static string? Of(IEnumerable<BridgeLineupEntryDto> lineup)
    {
        return Formation.Describe(lineup
            .Where(IsStarter)
            .Select(entry => entry.Position is { } mask ? PositionMasks.ToPosition(mask) : null)
            .OfType<TacticalPosition>());
    }
}
