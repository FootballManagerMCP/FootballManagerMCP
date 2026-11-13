using FootballManager.Api.Requests.Scouting;
using FootballManager.Api.Responses.Competitions;
using FootballManager.Api.Requests.Tactics.Editing;
using FootballManager.Api.Requests.Training;
using FootballManager.Api.Responses.Database;
using FootballManager.Api.Responses.Matches;
using FootballManager.Api.Responses.Messages;
using FootballManager.Api.Responses.Players;
using FootballManager.Api.Responses.Scouting;
using FootballManager.Api.Responses.Teams;
using FootballManager.Api.Responses.Trials;
using FootballManager.Api.Responses.Tactics.Effectiveness;
using FootballManager.Api.Responses.Training;
using FootballManager.Domain.Competitions;
using FootballManager.Domain.Database;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Messages;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using FootballManager.Domain.Tactics.Effectiveness;
using FootballManager.Domain.Teams;
using FootballManager.Domain.Training;
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
        RegisterMessages(config);
        RegisterTraining(config);
        RegisterCompetitions(config);

        config.NewConfig<DatabaseSearch, DatabaseSearchResponse>()
            .Map(dest => dest.Count, src => src.Results.Count);
    }

    // Flags that only repeat the usual case are left out: most sessions can't be changed, most players' intensity is
    // set by hand, and their focus and intensity are the user's to set. A request leaves out what it doesn't change.
    private static void RegisterTraining(TypeAdapterConfig config)
    {
        config.NewConfig<TrainingSession, TrainingSessionResponse>()
            .Map(dest => dest.CanEdit, src => src.CanEdit ? true : (bool?)null);

        config.NewConfig<PlayerTraining, PlayerTrainingResponse>()
            .Map(dest => dest.IntensityIsAutomatic, src => src.IntensityIsAutomatic ? true : (bool?)null)
            .Map(dest => dest.FocusLocked, src => src.CanEditFocus ? (bool?)null : true)
            .Map(dest => dest.IntensityLocked, src => src.CanEditIntensity ? (bool?)null : true);

        config.NewConfig<SessionChange, SessionChangeResponse>()
            .Map(dest => dest.From, src => src.Change.From)
            .Map(dest => dest.To, src => src.Change.To)
            .Map(dest => dest.Done, src => src.Change.Done);

        config.NewConfig<TrainingChanges, TrainingChangesResponse>()
            .Map(dest => dest.Notes, src => src.Notes.Count > 0 ? src.Notes : null);

        config.NewConfig<SetTrainingRequest, TrainingEdits>()
            .Map(dest => dest.Players, src => src.Players ?? new List<PlayerTrainingRequest>())
            .Map(dest => dest.Sessions, src => src.Sessions ?? new List<SessionRequest>());
    }

    // A player's stats come keyed by stat, which reads shorter than a list in long player lists.
    private static void RegisterCompetitions(TypeAdapterConfig config)
    {
        config.NewConfig<List<CompetitionInfo>, CompetitionListResponse>()
            .Map(dest => dest.Competitions, src => src);

        config.NewConfig<CompetitionInfo, CompetitionResponse>()
            .Map(dest => dest.IsUserLeague, src => src.IsUserLeague ? true : (bool?)null);

        config.NewConfig<CompetitionPlayer, CompetitionPlayerResponse>()
            .Map(dest => dest.PositionCodes, src => src.Positions)
            .Map(dest => dest.Stats, src => src.Stats.ToDictionary(
                stat => stat.Stat.ToString(),
                stat => stat.Adapt<CompetitionStatValueResponse>()));
    }

    // A message's flags are left out when false, except whether it's been read.
    private static void RegisterMessages(TypeAdapterConfig config)
    {
        config.NewConfig<Inbox, InboxResponse>()
            .Map(dest => dest.Count, src => src.Messages.Count);

        config.NewConfig<InboxMessage, InboxMessageResponse>()
            .Map(dest => dest.IsImportant, src => src.IsImportant ? true : (bool?)null)
            .Map(dest => dest.IsPriority, src => src.IsPriority ? true : (bool?)null)
            .Map(dest => dest.CanReply, src => src.CanReply ? true : (bool?)null)
            .Map(dest => dest.HasReplied, src => src.HasReplied ? true : (bool?)null)
            .Map(dest => dest.IsForwarded, src => src.IsForwarded ? true : (bool?)null);
    }

    // Flags and empty lists that only repeat the usual case are left out, keeping long player lists short.
    private static void RegisterScouting(TypeAdapterConfig config)
    {
        config.NewConfig<Shortlist, ShortlistResponse>()
            .Map(dest => dest.IsDefault, src => src.IsDefault ? true : (bool?)null)
            .Map(dest => dest.IsSelected, src => src.IsSelected ? true : (bool?)null);

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

        // Money stays in the game's own unit; the response names it.
        config.NewConfig<ScoutingBudget, ScoutingBudgetResponse>()
            .Map(dest => dest.Currency, src => ScoutingBudget.Currency);

        config.NewConfig<PlayerScoutingChange, PlayerScoutingChangeResponse>()
            .Map(dest => dest.ConfirmationIsWarning, src => src.ConfirmationIsWarning ? true : (bool?)null);

        // Every player of a team gets the same scout and duration, and mostly the same confirmation: they're given
        // once, and a player's own confirmation only when it says something else.
        config.NewConfig<TeamScoutingChange, TeamScoutingChangeResponse>()
            .Map(dest => dest.Count, src => src.Players.Count)
            .Map(dest => dest.Choice, src => src.Players.Count > 0 ? src.Players[0].Choice : null)
            .Map(dest => dest.Confirmation, src => src.Players.Count > 0 ? src.Players[0].Confirmation : null)
            .Map(dest => dest.Notes, src => src.Notes.Count > 0 ? src.Notes : null)
            .AfterMapping((src, dest) =>
            {
                for (var i = 0; i < dest.Players.Count && i < src.Players.Count; i++)
                {
                    if (SameConfirmation(src.Players[i], src.Players[0]))
                    {
                        dest.Players[i].Confirmation = null;
                    }
                }
            });

        config.NewConfig<PlayerScoutingChange, TeamScoutedPlayerResponse>()
            .Map(dest => dest.Name, src => src.PlayerName);

        config.NewConfig<SkippedPlayer, SkippedPlayerResponse>()
            .Map(dest => dest.Name, src => src.PlayerName);

        // As for a team's scouting: the entry and confirmation are given once, a player's own only when they differ.
        config.NewConfig<TrialChanges, TrialChangesResponse>()
            .Map(dest => dest.Count, src => src.Players.Count)
            .Map(dest => dest.Choice, src => src.Players.Count > 0 ? src.Players[0].Choice : null)
            .Map(dest => dest.Confirmation, src => src.Players.Count > 0 ? src.Players[0].Confirmation : null)
            .Map(dest => dest.Skipped, src => src.Skipped.Count > 0 ? src.Skipped : null)
            .Map(dest => dest.NotFound, src => src.NotFound.Count > 0 ? src.NotFound : null)
            .Map(dest => dest.Notes, src => src.Notes.Count > 0 ? src.Notes : null)
            .AfterMapping((src, dest) =>
            {
                for (var i = 0; i < dest.Players.Count && i < src.Players.Count; i++)
                {
                    if (SameConfirmation(src.Players[i], src.Players[0]))
                    {
                        dest.Players[i].Confirmation = null;
                    }

                    if (src.Players[i].Choice == src.Players[0].Choice)
                    {
                        dest.Players[i].Choice = null;
                    }
                }
            });

        config.NewConfig<PlayerScoutingChange, TrialPlayerResponse>()
            .Map(dest => dest.Name, src => src.PlayerName);
    }

    // The game names the player in his confirmation: two say the same when they do with the names left out.
    private static bool SameConfirmation(PlayerScoutingChange player, PlayerScoutingChange first)
    {
        return WithoutName(player) == WithoutName(first);
    }

    // string.Replace refuses an empty name.
    private static string? WithoutName(PlayerScoutingChange player)
    {
        return string.IsNullOrEmpty(player.PlayerName)
            ? player.Confirmation
            : player.Confirmation?.Replace(player.PlayerName, string.Empty);
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
        // Flags are left out when false.
        Rounded(config.NewConfig<MatchLineupPlayer, MatchLineupPlayerResponse>())
            .Map(dest => dest.WentOffInjured, src => src.WentOffInjured ? true : (bool?)null)
            .Map(dest => dest.YellowCard, src => src.YellowCard ? true : (bool?)null)
            .Map(dest => dest.RedCard, src => src.RedCard ? true : (bool?)null);

        config.NewConfig<MatchSubstitution, MatchSubstitutionResponse>()
            .Map(dest => dest.Injury, src => src.Injury ? true : (bool?)null);
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
