using System.Globalization;
using FootballManager.Domain.Matches;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Matches;
using Mapster;

namespace FootballManager.Infrastructure.Mappings;

// A match's incidents as the user's side reads them: which are goals (penalties and own goals among them), red cards
// and a penalty shoot-out's kicks. Each converts through its Mapster config, told whether the user's team was at home.
// The Mapster configs are the application's (the global ones) unless others are given.
internal static class MatchIncidents
{
    public static List<MatchGoal> Goals(IEnumerable<BridgeIncidentDto> incidents, bool isHome, TypeAdapterConfig? config = null)
    {
        return Adapt<MatchGoal>(incidents.Where(incident => KindOf(incident) is not null), isHome, config);
    }

    public static List<MatchRedCard> RedCards(IEnumerable<BridgeIncidentDto> incidents, bool isHome, TypeAdapterConfig? config = null)
    {
        return Adapt<MatchRedCard>(incidents.Where(incident => incident.IncidentType == MatchConstants.RedCardIncident), isHome, config);
    }

    public static PenaltyShootout? Shootout(IEnumerable<BridgeIncidentDto> incidents, bool isHome, TypeAdapterConfig? config = null)
    {
        var kicks = Adapt<PenaltyKick>(
            incidents.Where(incident => incident.IncidentType is MatchConstants.ShootoutScoredIncident or MatchConstants.ShootoutMissedIncident),
            isHome,
            config);

        if (kicks.Count == 0)
        {
            return null;
        }

        return new PenaltyShootout
        {
            Scored = kicks.Count(kick => kick.IsOurs && kick.Scored),
            Conceded = kicks.Count(kick => !kick.IsOurs && kick.Scored),
            Kicks = kicks
        };
    }

    /// <summary>When the given player was sent off, as a minute ("90+1'" is 91); null if he wasn't.</summary>
    public static int? SentOffMinute(IEnumerable<BridgeIncidentDto> incidents, int player)
    {
        var card = incidents.FirstOrDefault(incident => incident.IncidentType == MatchConstants.RedCardIncident && incident.Player?.Index == player);
        return card is null ? null : Minute(card.Minute);
    }

    // "21'" is 21 and "90+2'" is 92.
    public static int? Minute(string? text)
    {
        var parts = (text ?? string.Empty).Trim().TrimEnd('\'').Split('+');
        var total = 0;

        foreach (var part in parts)
        {
            if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var minutes))
            {
                return null;
            }

            total += minutes;
        }

        return total;
    }

    /// <summary>The kind of goal an incident is, or null when it isn't one.</summary>
    public static GoalKind? KindOf(BridgeIncidentDto incident)
    {
        return incident.IncidentType switch
        {
            MatchConstants.GoalIncident => GoalKind.Goal,
            MatchConstants.PenaltyGoalIncident => GoalKind.Penalty,
            MatchConstants.OwnGoalIncident => GoalKind.OwnGoal,
            _ => null
        };
    }

    private static List<T> Adapt<T>(IEnumerable<BridgeIncidentDto> incidents, bool isHome, TypeAdapterConfig? config)
    {
        return incidents.ToList()
            .BuildAdapter(config ?? TypeAdapterConfig.GlobalSettings)
            .AddParameters(MappingConfig.IsHomeParameter, isHome)
            .AdaptToType<List<T>>();
    }
}
