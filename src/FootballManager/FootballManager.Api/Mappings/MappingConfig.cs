using FootballManager.Api.Requests.Tactics.Editing;
using FootballManager.Api.Responses.Matches;
using FootballManager.Api.Responses.Players;
using FootballManager.Api.Responses.Tactics.Effectiveness;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Players;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using FootballManager.Domain.Tactics.Effectiveness;
using Mapster;

namespace FootballManager.Api.Mappings;

public class MappingConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Squad, SquadResponse>();

        config.NewConfig<Player, PlayerResponse>()
            .Map(dest => dest.Attributes, src => GroupAttributes(src.Attributes));

        Rounded(config.NewConfig<PlayerSeasonStats, PlayerSeasonStatsResponse>());

        // Scores and positions as FM writes them: "17" or "15-17", "DCL". Mapster inlines these, null sources included.
        config.NewConfig<Rating, string?>()
            .MapWith(src => src == null ? null : src.Display);

        config.NewConfig<TacticalPosition, string?>()
            .MapWith(src => src == null ? null : src.Code);

        config.NewConfig<TacticalEffectiveness, TacticalEffectivenessResponse>()
            .Map(dest => dest.Warnings, src => src.Warnings.Count > 0 ? src.Warnings : null);

        // A request leaves out what it doesn't change; an edit takes that as nothing to change.
        config.NewConfig<SetTacticRequest, TacticChanges>()
            .Map(dest => dest.TeamInstructions, src => src.TeamInstructions ?? new Dictionary<string, string>())
            .Map(dest => dest.Lineup, src => src.Lineup ?? new List<LineupChangeRequest>());

        RegisterMatches(config);
    }

    // A match reads from the user's side: "for" is their team, "against" the opponent.
    private static void RegisterMatches(TypeAdapterConfig config)
    {
        Rounded(config.NewConfig<MatchSummary, MatchSummaryResponse>())
            .Map(dest => dest.IsFriendly, src => src.IsFriendly ? true : (bool?)null)
            .Map(dest => dest.Opponent, src => src.Them.TeamName)
            .Map(dest => dest.OpponentLeaguePosition, src => src.Them.LeaguePosition)
            .Map(dest => dest.GoalsFor, src => src.Us.Goals)
            .Map(dest => dest.GoalsAgainst, src => src.Them.Goals)
            .Map(dest => dest.ExpectedGoalsFor, src => src.Us.ExpectedGoals)
            .Map(dest => dest.ExpectedGoalsAgainst, src => src.Them.ExpectedGoals)
            .Map(dest => dest.Possession, src => src.Us.Possession)
            .Map(dest => dest.ShotsFor, src => src.Us.Shots)
            .Map(dest => dest.ShotsAgainst, src => src.Them.Shots)
            .Map(dest => dest.ShotsOnTargetFor, src => src.Us.ShotsOnTarget)
            .Map(dest => dest.ShotsOnTargetAgainst, src => src.Them.ShotsOnTarget);

        Rounded(config.NewConfig<MatchRecord, MatchRecordResponse>());
        Rounded(config.NewConfig<TeamMatchStats, TeamMatchStatsResponse>());
        Rounded(config.NewConfig<PlayerMatchStats, PlayerMatchStatsResponse>());
        Rounded(config.NewConfig<MatchLineupPlayer, MatchLineupPlayerResponse>());
        Rounded(config.NewConfig<PlayerOfTheMatch, PlayerOfTheMatchResponse>());
    }

    // The game keeps full precision; two decimals is all a reader needs.
    private static TypeAdapterSetter<TSource, TDestination> Rounded<TSource, TDestination>(
        TypeAdapterSetter<TSource, TDestination> setter)
    {
        return setter
            .AddDestinationTransform((double value) => Math.Round(value, 2))
            .AddDestinationTransform((double? value) => value.HasValue ? Math.Round(value.Value, 2) : value);
    }

    // Grouped by category with FM's own notation, which is far more compact than one object per attribute when a
    // whole squad comes back.
    public static Dictionary<string, Dictionary<string, string?>>? GroupAttributes(List<PlayerAttribute> attributes)
    {
        if (attributes.Count == 0)
        {
            return null;
        }

        return attributes
            .GroupBy(a => a.Category)
            .ToDictionary(
                group => group.Key.ToString().ToLowerInvariant(),
                group => group.ToDictionary(a => a.Name, a => a.Display));
    }
}
