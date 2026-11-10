using FootballManager.Api.Requests.Scouting;
using FootballManager.Api.Requests.Tactics.Editing;
using FootballManager.Api.Responses.Matches;
using FootballManager.Api.Responses.Players;
using FootballManager.Api.Responses.Scouting;
using FootballManager.Api.Responses.Teams;
using FootballManager.Api.Responses.Tactics.Effectiveness;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using FootballManager.Domain.Tactics.Effectiveness;
using FootballManager.Domain.Teams;
using Mapster;

namespace FootballManager.Api.Mappings;

public class MappingConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Squad, SquadResponse>();

        config.NewConfig<Player, PlayerResponse>()
            .Map(dest => dest.Statuses, src => src.Statuses.Count > 0 ? src.Statuses : null)
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
        RegisterScouting(config);
    }

    // Flags and empty lists that only repeat the usual case are left out, keeping long player lists short.
    private static void RegisterScouting(TypeAdapterConfig config)
    {
        config.NewConfig<ScoutedPlayerList, ScoutedPlayerListResponse>()
            .Map(dest => dest.Count, src => src.Players.Count)
            .Map(dest => dest.HiddenDataMasked, src => src.HiddenDataMasked ? true : (bool?)null);

        config.NewConfig<ScoutedPlayer, ScoutedPlayerResponse>()
            .Map(dest => dest.KnowledgeLevel, src => src.KnowledgeLevel == null ? null : $"{src.KnowledgeLevel}/{src.KnowledgeMax}")
            .Map(dest => dest.IsShortlisted, src => src.IsShortlisted ? true : (bool?)null)
            .Map(dest => dest.IsBeingScouted, src => src.IsBeingScouted ? true : (bool?)null)
            .Map(dest => dest.Statuses, src => src.Statuses.Count > 0 ? src.Statuses : null)
            .Map(dest => dest.Attributes, src => GroupAttributes(src.Attributes));

        config.NewConfig<ScoutReport, ScoutReportResponse>()
            .Map(dest => dest.Pros, src => src.Pros.Count > 0 ? src.Pros : null)
            .Map(dest => dest.Cons, src => src.Cons.Count > 0 ? src.Cons : null);

        // A request leaves out what it doesn't set: no areas, any position.
        config.NewConfig<CreateFocusRequest, FocusBrief>()
            .Map(dest => dest.Areas, src => src.Areas ?? new List<string>())
            .Map(dest => dest.Positions, src => src.Positions ?? new List<string>());

        // A search request leaves out what it doesn't filter on.
        config.NewConfig<RunPlayerSearchRequest, PlayerSearchCriteria>()
            .Map(dest => dest.QuickFilters, src => src.QuickFilters ?? new List<SearchQuickFilter>())
            .Map(dest => dest.Positions, src => src.Positions ?? new List<PitchPosition>());

        config.NewConfig<PlayerSearchCriteria, PlayerSearchCriteriaResponse>()
            .Map(dest => dest.QuickFilters, src => src.QuickFilters.Count > 0 ? src.QuickFilters : null)
            .Map(dest => dest.Positions, src => src.Positions.Count > 0 ? src.Positions : null);

        config.NewConfig<PlayerSearchChange, PlayerSearchChangeResponse>()
            .Map(dest => dest.Notes, src => src.Notes.Count > 0 ? src.Notes : null);

        config.NewConfig<FocusCreation, FocusCreationResponse>()
            .Map(dest => dest.Planned, src => src.Draft)
            .Map(dest => dest.Notes, src => src.Notes.Count > 0 ? src.Notes : null);

        config.NewConfig<FocusDraft, FocusDraftResponse>()
            .Map(dest => dest.Areas, src => src.Areas.Count > 0 ? src.Areas.Select(a => a.Display).ToList() : null)
            .Map(dest => dest.Positions, src => src.Positions.Count > 0 ? src.Positions.Select(p => p.Code).ToList() : null);

        config.NewConfig<RecruitmentFocus, RecruitmentFocusResponse>()
            .Map(dest => dest.ShowsRemovedFocus, src => src.ShowsRemovedFocus ? true : (bool?)null)
            .Map(dest => dest.IsComplete, src => src.IsComplete ? true : (bool?)null)
            .Map(dest => dest.IsStruggling, src => src.IsStruggling ? true : (bool?)null)
            .Map(dest => dest.Positions, src => src.Positions.Count > 0 ? src.Positions.Select(p => p.Code).ToList() : null)
            .Map(dest => dest.OutOfPossessionPositions, src => src.OutOfPossessionPositions.Count > 0
                ? src.OutOfPossessionPositions.Select(p => p.Code).ToList()
                : null);
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

        Rounded(config.NewConfig<StyleAverages, StyleAveragesResponse>());
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
