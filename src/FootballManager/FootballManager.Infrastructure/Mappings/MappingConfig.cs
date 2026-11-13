using System.Globalization;
using FootballManager.Domain.Competitions;
using FootballManager.Domain.Database;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Messages;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using FootballManager.Domain.Tactics.Effectiveness;
using FootballManager.Domain.Training;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Competitions;
using FootballManager.Infrastructure.Dtos.Database;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Dtos.Messages;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Dtos.Tactics;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;
using FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;
using FootballManager.Infrastructure.Dtos.Training;
using Mapster;

namespace FootballManager.Infrastructure.Mappings;

public class MappingConfig
{
    /// <summary>The Mapster parameter giving a phase's role names by mask, for a role's key (TacticKeys.Roles).</summary>
    public const string RoleKeysParameter = "roleKeys";

    /// <summary>The Mapster parameter giving the game's names for its tactic ids (TacticKeys), for a scouted player's roles.</summary>
    public const string TacticKeysParameter = "tacticKeys";

    /// <summary>The Mapster parameter giving the game's names for a player's status flags (GameVocabulary.StatusKeysAsync).</summary>
    public const string StatusKeysParameter = "statusKeys";

    private const string UnknownAmount = "-";

    // What the game sends for a player it has never scouted: 30000 days since (about 82 years), a report dated 1900.
    private const int NeverScoutedDays = 30000;
    private const string BlankYear = "1900";

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BridgePlayerDto, Player>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Condition, src => src.Condition >= 0 ? src.Condition : null)
            .Map(dest => dest.IsInjured, src => src.IsInjured ?? false)
            // The game sends 0 for a player without appearances.
            .Map(dest => dest.AverageRating, src => src.AverageRating > 0 ? (double?)Math.Round(src.AverageRating.Value, 2) : null)
            // Status keys need the game's names for the flags, passed as StatusKeysParameter.
            .Map(dest => dest.Statuses, src => AllStatuses(src.Statuses, src.TeamStatuses))
            .Map(dest => dest.TrialUntil, src => src.TrialIsValid == true ? src.TrialEndDate : null)
            .Map(dest => dest.Attributes, src => AttributeBoundsMapper.ToAttributes(src.AttributeBounds))
            // Appearances always comes back when the season's numbers were asked for.
            .Map(dest => dest.SeasonStats, src => src.Appearances == null ? null : src);

        config.NewConfig<BridgePlayerDto, PlayerSeasonStats>();

        RegisterTactics(config);
        RegisterMatches(config);
        RegisterScouting(config);
        RegisterMessages(config);
        RegisterDatabase(config);
        RegisterTraining(config);
        RegisterCompetitions(config);
    }

    // A competition's stats screen, and the contract of a player in it as the game shows the user.
    private static void RegisterCompetitions(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeCompetitionDto, CompetitionInfo>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Nation, src => Text(src.Nation))
            // Set by the client, which knows which league is the user's.
            .Ignore(dest => dest.IsUserLeague);

        config.NewConfig<BridgeStatRowDto, CompetitionStatRow>()
            .Map(dest => dest.PlayerId, src => src.PlayerId ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Team, src => Text(src.Team))
            .Map(dest => dest.Positions, src => src.Positions == null ? new List<PitchPosition>() : PositionMasks.All(src.Positions.Value))
            .Map(dest => dest.Value, src => Math.Round(src.Value ?? 0, 2))
            // Set by the client from the rows' order.
            .Ignore(dest => dest.Rank)
            .Ignore(dest => dest.RowIndex);

        // A contract without an end (amateur and non-contract terms) is dated 1900; a wage that doesn't apply reads "N/A"
        // with 0, and a club that hasn't set an asking price sends -1.
        config.NewConfig<BridgePlayerContractDto, PlayerContract>()
            .Map(dest => dest.Status, src => src.Status == null ? null : Text(src.Status.DisplayValue))
            .Map(dest => dest.Type, src => src.Type == null ? null : Text(src.Type.DisplayValue))
            .Map(dest => dest.EndDate, src => src.HasEndDate == true && !IsBlankDate(src.EndDate) ? src.EndDate : null)
            .Map(dest => dest.Wage, src => src.Wage != null && src.Wage.PropertyValue > 0 ? Text(src.Wage.DisplayValue) : null)
            .Map(dest => dest.Value, src => src.Value == null ? null : Text(src.Value.TransferValueRangeString))
            .Map(dest => dest.AskingPrice, src => src.Value != null && src.Value.AskingPrice > 0 ? src.Value.AskingPrice : null)
            .Map(dest => dest.TransferStatus, src => src.TransferStatus == null || src.TransferStatus.TransferStatusDescription == null
                ? null
                : Text(src.TransferStatus.TransferStatusDescription.Text));
    }

    // A player's training as his individual training screen shows it: the position and role he trains in (those he plays
    // unless picked otherwise), and the dropdowns' current rows. "---" is the game's unset row; a focus of -1 is none.
    // His role key needs the in-possession role names, passed as RoleKeysParameter. A calendar day holds its sessions
    // and, on a match day, the fixture as an event of its own.
    private static void RegisterTraining(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeOptionDto, TrainingOption>()
            .Map(dest => dest.Id, src => src.PropertyValue ?? 0)
            .Map(dest => dest.Name, src => src.DisplayValue ?? string.Empty);

        config.NewConfig<BridgeOptionDto, TrainingSessionCategory>()
            .Map(dest => dest.Id, src => (int)(src.PropertyValue ?? 0))
            .Map(dest => dest.Name, src => src.DisplayValue ?? string.Empty)
            .Ignore(dest => dest.Sessions);

        config.NewConfig<BridgePlayerTrainingDto, PlayerTraining>()
            .Map(dest => dest.PlayerId, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Position, src => PositionCode(src.Position))
            .Map(dest => dest.RoleKey, src => RoleKey(src.RoleId))
            .Map(dest => dest.RoleName, src => Text(src.RoleName))
            .Map(dest => dest.FollowsMatchRole, src => src.PositionChoice == TrainingConstants.MatchPosition)
            .Map(dest => dest.OutOfPossessionRole, src => Chosen(src.OutOfPossessionRole))
            .Map(dest => dest.Focus, src => src.Focus == null || src.Focus.PropertyValue == null || src.Focus.PropertyValue < 0 ? null : src.Focus)
            .Map(dest => dest.Intensity, src => src.Intensity == null || src.Intensity.PropertyValue == null ? null : src.Intensity)
            .Map(dest => dest.IntensityIsAutomatic, src => src.IntensityAutomatic == true)
            .Map(dest => dest.RestDays, src => src.RestDays > 0 ? src.RestDays : null)
            // The game sends 0 for a player who hasn't trained lately (injured, say).
            .Map(dest => dest.Rating, src => src.Rating > 0 ? (double?)Math.Round(src.Rating.Value, 2) : null)
            .Map(dest => dest.Workload, src => Text(src.Workload))
            .Map(dest => dest.CanEditFocus, src => src.FocusCanEdit == true && src.FocusHumanControlled == true)
            .Map(dest => dest.CanEditIntensity, src => src.IntensityCanEdit == true)
            .Map(dest => dest.FocusChoices, src => TrainingRows.Of(src.FocusChoices).Select(row => row.PropertyValue!.Value).ToList())
            .Map(dest => dest.IntensityChoices, src => TrainingRows.Of(src.IntensityChoices, TrainingConstants.Holidays)
                .Select(row => row.PropertyValue!.Value).ToList());

        config.NewConfig<BridgeTrainingWeekDto, TrainingWeek>()
            .Map(dest => dest.StartDate, src => Text(src.WeekStartDate))
            .Map(dest => dest.Days, src => src.Days().Where(day => day.Day != null).Select(day => day.Day!).ToList());

        config.NewConfig<BridgeTrainingDayDto, TrainingDay>()
            .Map(dest => dest.Date, src => src.Date ?? string.Empty)
            .Map(dest => dest.Sessions, src => (src.Events ?? new List<BridgeCalendarEventDto>())
                .Where(e => e.SessionName != null && e.SessionIndex != null).ToList())
            .Map(dest => dest.Match, src => (src.Events ?? new List<BridgeCalendarEventDto>())
                .Where(e => e.CalendarEvent == TrainingConstants.MatchEvent).Select(e => Text(e.MatchVenue)).FirstOrDefault());

        config.NewConfig<BridgeCalendarEventDto, TrainingSession>()
            .Map(dest => dest.Slot, src => src.SessionIndex ?? 0)
            .Map(dest => dest.Type, src => src.SessionType ?? 0)
            .Map(dest => dest.Name, src => src.SessionName ?? string.Empty)
            .Map(dest => dest.Category, src => src.SessionCategory ?? -1)
            .Map(dest => dest.CanEdit, src => src.CanEdit == true);
    }

    private static string? PositionCode(ulong? mask)
    {
        return mask is { } position ? PositionMasks.ToPosition(position)?.Code : null;
    }

    // A dropdown's current row, unless it's the unset one.
    private static string? Chosen(BridgeOptionDto? row)
    {
        return row?.DisplayValue is { } label && label != TrainingConstants.Unset ? Text(label) : null;
    }

    // What the search shows of each kind: a person's club and nationality, a club's nation and league, a nation's
    // continent. The linked records are the search's own line under the name.
    private static void RegisterDatabase(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeDatabaseRecordDto, DatabaseRecord>()
            .Map(dest => dest.Kind, src => RecordKind(src))
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.LongName, src => RecordKind(src) == DatabaseRecordKind.Club && src.LongName != src.Name ? Text(src.LongName) : null)
            .Map(dest => dest.Description, src => Text(src.Description))
            .Map(dest => dest.Type, src => RecordKind(src) == DatabaseRecordKind.Other || RecordKind(src) == DatabaseRecordKind.NationalTeam
                ? Text(src.TypeLabel)
                : null)
            .Map(dest => dest.Age, src => IsPerson(src) ? src.Age : null)
            .Map(dest => dest.Positions, src => RecordKind(src) == DatabaseRecordKind.Player ? Text(src.Positions) : null)
            .Map(dest => dest.Job, src => RecordKind(src) == DatabaseRecordKind.Staff ? Text(src.Job) : null)
            .Map(dest => dest.Nation, src => RecordNation(src))
            .Map(dest => dest.Club, src => IsPerson(src) ? Text(src.Club) : null)
            .Map(dest => dest.ClubId, src => IsPerson(src) ? src.ClubId : null)
            .Map(dest => dest.ClubNation, src => IsPerson(src) ? Text(src.ClubNation) : null)
            .Map(dest => dest.League, src => RecordLeague(src))
            .Map(dest => dest.Continent, src => RecordContinent(src));
    }

    // Every message attaches something: a forwarded one the article it passes on, any other itself. Only a forwarded
    // message's source and attachment are its own.
    private static void RegisterMessages(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeInboxMessageDto, InboxMessage>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Title, src => src.Title ?? string.Empty)
            .Map(dest => dest.Sender, src => Text(src.Sender))
            .Map(dest => dest.Subtitle, src => Text(src.Subtitle))
            .Map(dest => dest.Body, src => Text(src.Body))
            .Map(dest => dest.IsRead, src => src.IsRead == true)
            .Map(dest => dest.IsImportant, src => src.IsImportant == true)
            .Map(dest => dest.IsPriority, src => src.IsPriority == true)
            .Map(dest => dest.CanReply, src => src.CanReply == true)
            .Map(dest => dest.HasReplied, src => src.HasReplied == true)
            .Map(dest => dest.IsForwarded, src => src.IsForwarded == true)
            .Map(dest => dest.Source, src => src.IsForwarded == true ? Text(src.Source) : null)
            .Map(dest => dest.Article, src => src.IsForwarded == true ? Text(src.Article) : null);
    }

    // What the club's scouting knows: the game sends values it doesn't know as empty text, or a -1 count.
    private static void RegisterScouting(TypeAdapterConfig config)
    {
        // A position is an immutable record: kept as it is rather than copied (it has no default constructor).
        config.NewConfig<TacticalPosition, TacticalPosition>().MapWith(src => src);

        // Role keys need the game's role names (TacticKeys), passed as TacticKeysParameter.
        config.NewConfig<BridgeScoutedPlayerDto, ScoutedPlayer>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Club, src => Text(src.Club))
            .Map(dest => dest.Nation, src => Text(src.Nation))
            .Map(dest => dest.Recommendation, src => Text(src.Recommendation))
            .Map(dest => dest.GradeLabel, src => Text(src.GradeLabel))
            .Map(dest => dest.BestPosition, src => src.BestPosition == null ? null : PositionMasks.ToPosition(src.BestPosition.Value))
            .Map(dest => dest.BestRole, src => ScoutedRoleKey(src.BestRole, true))
            .Map(dest => dest.BestOutOfPossessionRole, src => ScoutedRoleKey(src.BestOutOfPossessionRole, false))
            .Map(dest => dest.KnowledgeLevel, src => src.Knowledge == null ? null : src.Knowledge.ActualValue)
            .Map(dest => dest.KnowledgeMax, src => src.Knowledge == null ? null : src.Knowledge.MaxValue)
            .Map(dest => dest.Knowledge, src => src.Knowledge == null ? null : Text(src.Knowledge.Text))
            .Map(dest => dest.Value, src => Range(src.MinValue, src.MaxValue))
            .Map(dest => dest.Wage, src => Range(Display(src.MinWage), Display(src.MaxWage)))
            .Map(dest => dest.StrongestArea, src => Text(src.StrongestArea))
            .Map(dest => dest.WeakestArea, src => Text(src.WeakestArea))
            .Map(dest => dest.IsShortlisted, src => src.IsShortlisted == true)
            .Map(dest => dest.IsBeingScouted, src => src.IsBeingScouted == true)
            .Map(dest => dest.DaysSinceLastScouted, src => Days(src.DaysSinceLastScouted))
            .Map(dest => dest.Statuses, src => AllStatuses(src.Statuses, src.TeamStatuses))
            .Map(dest => dest.Attributes, src => AttributeBoundsMapper.ToAttributes(src.AttributeBounds))
            // The report's date always comes back when the report was asked for; a player never scouted has a blank one.
            .Map(dest => dest.Report, src => IsBlankDate(src.ReportDate) ? null : src);

        config.NewConfig<BridgeScoutedPlayerDto, ScoutReport>()
            .Map(dest => dest.Date, src => src.ReportDate)
            .Map(dest => dest.Scout, src => Text(src.ReportScout))
            .Map(dest => dest.Ability, src => Text(src.ReportAbility))
            .Map(dest => dest.Potential, src => Text(src.ReportPotential))
            .Map(dest => dest.Pros, src => src.ReportProsCons == null ? new List<string>() : Sentences(src.ReportProsCons.ProsList))
            .Map(dest => dest.Cons, src => src.ReportProsCons == null ? new List<string>() : Sentences(src.ReportProsCons.ConsList))
            .Map(dest => dest.TransferFee, src => src.ReportTransfer == null ? null : Short(src.ReportTransfer.ExpectedTransferValue))
            .Map(dest => dest.WageDemands, src => src.ReportTransfer == null ? null : Short(src.ReportTransfer.ExpectedWageDemands))
            .Map(dest => dest.TransferInterest, src => src.ReportTransfer == null ? null : Short(src.ReportTransfer.ExpectedTransferInterest))
            .Map(dest => dest.LoanInterest, src => src.ReportTransfer == null ? null : Short(src.ReportTransfer.ExpectedLoanInterest));

        config.NewConfig<BridgeRecruitmentFocusDto, RecruitmentFocus>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.IsComplete, src => src.IsComplete == true)
            .Map(dest => dest.IsStruggling, src => src.IsStruggling == true)
            .Map(dest => dest.Advice, src => Text(src.Advice))
            .Map(dest => dest.Areas, src => Text(src.Areas))
            .Map(dest => dest.Scouts, src => Text(src.Scouts))
            .Map(dest => dest.MinAge, src => src.MinAge == null ? null : (int?)src.MinAge.PropertyValue)
            .Map(dest => dest.MaxAge, src => src.MaxAge == null ? null : (int?)src.MaxAge.PropertyValue)
            .Map(dest => dest.MinAbilityStars, src => src.MinAbility)
            .Map(dest => dest.MinPotentialStars, src => src.MinPotential)
            .Map(dest => dest.Positions, src => TickedPositions(src.Positions, true))
            .Map(dest => dest.OutOfPossessionPositions, src => TickedPositions(src.Positions, false))
            .Map(dest => dest.RecommendedPlayers, src => src.RecommendedCount)
            .Map(dest => dest.CurrentPlayers, src => src.CurrentCount)
            .Map(dest => dest.NearMissPlayers, src => src.NearMissCount)
            // Set by the client from what the API removed (RemovedFocusLog), not by the game.
            .Ignore(dest => dest.ShowsRemovedFocus);

        config.NewConfig<BridgePlayerStatusDto, PlayerStatus>()
            .Map(dest => dest.Key, src => StatusKey(src.StatusFlag))
            .Map(dest => dest.Label, src => Text(src.StatusTitleFull))
            .Map(dest => dest.Detail, src => OneLine(src.StatusReason));

        config.NewConfig<BridgeSearchStatusDto, PlayerSearchStatus>()
            .Map(dest => dest.Status, src => Text(src.Status))
            .Map(dest => dest.Interest, src => Text(src.Interest))
            .Map(dest => dest.Attributes, src => Text(src.Attributes))
            // Read on their own (IPlayerSearchGameClient).
            .Ignore(dest => dest.Filters!);

        config.NewConfig<BridgeSearchFiltersDto, PlayerSearchFilters>()
            .Map(dest => dest.InterestType, src => InterestType(src.State.Interest))
            .Map(dest => dest.InterestLevel, src => InterestLevel(src.State.Interest))
            .Map(dest => dest.OtherConditions, src => OtherConditions(src));

        config.NewConfig<BridgeSearchRuleDto, SearchRule>()
            .Map(dest => dest.Name, src => Text(src.Name))
            .Map(dest => dest.Comparison, src => src.Comparison == null ? null : Text(src.Comparison.Text))
            .Map(dest => dest.Value, src => RuleValue(src));

        // A rule's values are its options' ids: ages as they are, positions as masks, grades by their letters.
        config.NewConfig<BridgeSearchChoicesDto, PlayerSearchChoices>()
            .Map(dest => dest.Ages, src => (src.Age.MinValues ?? new List<BridgeFilterItemDto>())
                .Where(item => item.Id != null).Select(item => (int)item.Id!.Value).ToList())
            .Map(dest => dest.Positions, src => SearchPositions(src.Position.Values))
            .Map(dest => dest.Familiarities, src => Familiarities(src.Position.Comparisons))
            .Map(dest => dest.Grades, src => (src.Grade.MinValues ?? new List<BridgeFilterItemDto>())
                .Where(item => item.Id != PlayerSearchConstants.AnyGrade && item.Name != null).Select(item => item.Name!).ToList());

        // A top entry carries its own fields, an entry below has them in RowData; both keep their sub-entries in ChildList.
        config.NewConfig<BridgeMenuItemDto, MenuEntry>()
            .Map(dest => dest.Label, src => MenuLabel(src))
            .Map(dest => dest.CanRun, src => MenuRuns(src))
            .Map(dest => dest.Confirmation, src => MenuConfirmation(src))
            .Map(dest => dest.ConfirmationIsWarning, src => MenuWarns(src))
            .Map(dest => dest.Children, src => src.ChildList ?? new List<BridgeMenuItemDto>());

        // Every box of the position grid, ticked or not, is a position a focus can ask for.
        config.NewConfig<BridgeDraftFocusDto, FocusChoices>()
            .Map(dest => dest.MinAges, src => Ages(src.MinAgeChoices))
            .Map(dest => dest.MaxAges, src => Ages(src.MaxAgeChoices))
            .Map(dest => dest.Positions, src => GridPositions(src.Positions));

        config.NewConfig<BridgeScoutDto, Scout>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty);

        config.NewConfig<BridgeScoutingCountsDto, ScoutingOverview>()
            .Map(dest => dest.RecommendedPlayers, src => src.Centre == null ? null : src.Centre.RecommendedPlayerCount)
            .Map(dest => dest.HighlyRecommendedPlayers, src => src.Centre == null ? null : src.Centre.HighlyRecommendedPlayersCount)
            .Map(dest => dest.RequestedReports, src => src.Centre == null ? null : src.Centre.RequestedReportsCount)
            .Map(dest => dest.FocusesShown, src => src.FocusListType == null ? null : (FocusListFilter?)src.FocusListType.Value)
            .Ignore(dest => dest.Scouts, dest => dest.Focuses, dest => dest.Shortlists);

        // A shortlist's size comes as text ("14 players"); which one the screen shows is set by the client.
        config.NewConfig<BridgeShortlistDto, Shortlist>()
            .Map(dest => dest.Name, src => Text(src.DisplayString) ?? string.Empty)
            .Map(dest => dest.Players, src => LeadingNumber(src.CountString))
            .Map(dest => dest.IsDefault, src => src.IsDefault == true)
            .Ignore(dest => dest.IsSelected);
    }

    private static int? LeadingNumber(string? text)
    {
        var digits = new string((text ?? string.Empty).SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        return digits.Length > 0 ? int.Parse(digits, CultureInfo.InvariantCulture) : null;
    }

    private static void RegisterMatches(TypeAdapterConfig config)
    {
        // A match's score and numbers only mean something once it's played: before that the game sends -1 and zeros.
        // Which side is the user's, and the shape they played, are set by the client.
        config.NewConfig<BridgeFixtureDto, MatchSummary>()
            .Map(dest => dest.Date, src => DateOnly.ParseExact(src.Date!, MatchConstants.DateFormat, CultureInfo.InvariantCulture))
            .Map(dest => dest.IsFriendly, src => src.IsFriendly == true)
            .Map(dest => dest.IsPlayed, src => src.HasResult == true)
            .Map(dest => dest.Home, src => new MatchSide
            {
                TeamId = src.HomeTeamId ?? 0,
                TeamName = src.HomeTeamName ?? string.Empty,
                Goals = src.HasResult == true ? src.HomeScore : null,
                ExpectedGoals = src.HasResult == true ? src.HomeExpectedGoals : null,
                Possession = src.HasResult == true ? src.HomePossession : null,
                Shots = src.HasResult == true ? src.HomeShots : null,
                ShotsOnTarget = src.HasResult == true ? src.HomeShotsOnTarget : null,
                LeaguePosition = string.IsNullOrEmpty(src.HomeLeaguePosition) ? null : src.HomeLeaguePosition
            })
            .Map(dest => dest.Away, src => new MatchSide
            {
                TeamId = src.AwayTeamId ?? 0,
                TeamName = src.AwayTeamName ?? string.Empty,
                Goals = src.HasResult == true ? src.AwayScore : null,
                ExpectedGoals = src.HasResult == true ? src.AwayExpectedGoals : null,
                Possession = src.HasResult == true ? src.AwayPossession : null,
                Shots = src.HasResult == true ? src.AwayShots : null,
                ShotsOnTarget = src.HasResult == true ? src.AwayShotsOnTarget : null,
                LeaguePosition = string.IsNullOrEmpty(src.AwayLeaguePosition) ? null : src.AwayLeaguePosition
            })
            .Ignore(dest => dest.TeamId, dest => dest.Formation!);

        config.NewConfig<BridgeTeamMatchStatsDto, TeamMatchStats>()
            .Map(dest => dest.Possession, src => src.TotalPossession)
            .Map(dest => dest.Shots, src => src.TotalShots)
            .Map(dest => dest.ShotsHitWoodwork, src => src.ShotHitWoodwork)
            .Map(dest => dest.Crosses, src => src.CrossAttempts)
            .Map(dest => dest.Passes, src => src.PassAttempts)
            .Map(dest => dest.PassesPerDefensiveAction, src => src.OppositionPassesPerDefensiveAction)
            .Map(dest => dest.Tackles, src => src.TackleAttempts)
            .Map(dest => dest.Headers, src => src.HeaderAttempts)
            .Map(dest => dest.Fouls, src => src.FoulsMade)
            .Map(dest => dest.AverageRating, src => src.AverageRatingFloat);

        config.NewConfig<BridgePlayerMatchStatsDto, PlayerMatchStats>()
            .Map(dest => dest.Shots, src => src.TotalShots)
            .Map(dest => dest.Passes, src => src.PassAttempts)
            .Map(dest => dest.Dribbles, src => src.DribblesMade)
            .Map(dest => dest.Crosses, src => src.CrossAttempts)
            .Map(dest => dest.Tackles, src => src.TackleAttempts)
            .Map(dest => dest.TacklesWon, src => src.TackleCompleted)
            .Map(dest => dest.AerialDuels, src => src.AerialDuelsAttempted)
            .Map(dest => dest.AerialDuelsWon, src => src.AerialDuelsCompleted)
            .Map(dest => dest.Saves, src => src.TotalSaves)
            .Map(dest => dest.DistanceRun, src => src.DistanceRan);
    }

    private static void RegisterTactics(TypeAdapterConfig config)
    {
        // The game sends -1 for a score it doesn't know (e.g. a role rating for a player it can't judge); a score not
        // asked for comes as no record at all.
        config.NewConfig<BridgeRatingDto, Rating?>()
            .MapWith(src => src != null && src.MaxValue >= 0
                ? new Rating(src.MinValue >= 0 ? src.MinValue!.Value : src.MaxValue!.Value, src.MaxValue!.Value)
                : null);

        config.NewConfig<BridgeAdviceDto, EffectivenessAdvice>()
            .Map(dest => dest.Text, src => src.Text ?? string.Empty)
            .Map(dest => dest.IsPositive, src => src.IsPro ?? false);

        config.NewConfig<BridgeSlotEffectivenessDto, TacticalEffectiveness>()
            .Map(dest => dest.Warnings, src => src.StatusFlags.Where(flag => flag.Value).Select(flag => flag.Key).ToList());

        config.NewConfig<BridgePlayerAbilityDto, PlayerAbilityScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.RoleSuitability, src => src.RoleSuitabilityScore)
            .Map(dest => dest.Capability, src => src.CapabilityScore)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        config.NewConfig<BridgePlayerBuyInDto, PlayerBuyInScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.ManagerSupport, src => src.ManagerSupportScore)
            .Map(dest => dest.Relationships, src => src.TeammateAndStaffRelationshipsScore)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        config.NewConfig<BridgeTacticalUnderstandingDto, TacticalUnderstandingScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.Comprehension, src => src.TacticalComprehension)
            .Map(dest => dest.ComprehensionAdvice, src => src.AdviceForTacticalComprehension)
            .Map(dest => dest.AreasAdvice, src => src.AdviceForTacticalAreas)
            .Map(dest => dest.Complexity, src => src.TacticalComplexity);

        config.NewConfig<BridgeTacticalAreaDto, TacticalArea>()
            .Map(dest => dest.Understanding, src => src.OverallEffectiveness);

        config.NewConfig<BridgeTacticalComplexityDto, TacticalComplexity>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.TeamInstructions, src => src.TeamInstructionComplexity)
            .Map(dest => dest.PlayerInstructions, src => src.PlayerInstructionComplexity)
            .Map(dest => dest.OppositionInstructions, src => src.OppositionInstructionComplexity);

        config.NewConfig<BridgeNextMatchDto, NextMatchScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        // A role's key needs the phase's role names (TacticKeys.Roles), passed as RoleKeysParameter.
        config.NewConfig<BridgeRoleRowDto, RoleRating>()
            .Map(dest => dest.Key, src => RoleKey(src.TacticsRoleId))
            .Map(dest => dest.Role, src => src.TacticsRoleName ?? string.Empty)
            .Map(dest => dest.Rating, src => src.CurrentAbilityStars);

        RegisterTacticEditing(config);

        // Index and position label come from the slot's position list, not the player.
        config.NewConfig<BridgeSlotPlayerDto, SlotPosition>()
            .Map(dest => dest.PlayerId, src => src.Id)
            .Map(dest => dest.PlayerName, src => src.Name)
            .Ignore(dest => dest.Index, dest => dest.InPossession!);
    }

    // The tactics screen's catalogs, as tactic edits name things: the game's identifiers as keys, its labels as labels.
    private static void RegisterTacticEditing(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeInstructionOptionDto, TacticOption>()
            .MapWith(src => new TacticOption(src.Option ?? string.Empty, src.Name));

        // The option the tactic holds comes from the tactic, not the catalog.
        config.NewConfig<BridgeTeamInstructionDto, TeamInstructionOptions>()
            .Map(dest => dest.Key, src => src.Instruction ?? string.Empty)
            .Map(dest => dest.Label, src => src.Name)
            .Ignore(dest => dest.Current!);

        config.NewConfig<BridgePlayerInstructionGroupDto, PlayerInstructionGroup>()
            .Map(dest => dest.Key, src => src.Group ?? string.Empty)
            .Map(dest => dest.Label, src => src.Name)
            .Map(dest => dest.InPossession, src => !TacticConstants.OutOfPossessionInstructionGroups.Contains(src.Group))
            .Map(dest => dest.Options, src => src.Options
                .Where(option => option.Option != null)
                .Select(option => new TacticOption(TacticKeys.PlayerInstructionKey(option.Option!), option.Name))
                .ToList());
    }

    private static string? ScoutedRoleKey(ulong? mask, bool inPossession)
    {
        return mask is { } role
               && MapContext.Current?.Parameters.TryGetValue(TacticKeysParameter, out var names) == true
               && names is TacticKeys keys
            ? keys.Roles(inPossession).GetValueOrDefault(role)
            : null;
    }

    private static DatabaseRecordKind RecordKind(BridgeDatabaseRecordDto record)
    {
        if (IsPerson(record))
        {
            return record.IsPlayer switch
            {
                true => DatabaseRecordKind.Player,
                false => DatabaseRecordKind.Staff,
                null => DatabaseRecordKind.Other
            };
        }

        return record.RecordType is { } type
            ? DatabaseSearchConstants.Kinds.GetValueOrDefault(type, DatabaseRecordKind.Other)
            : DatabaseRecordKind.Other;
    }

    private static bool IsPerson(BridgeDatabaseRecordDto record)
    {
        return record.RecordType == DatabaseSearchConstants.Person;
    }

    private static string? RecordNation(BridgeDatabaseRecordDto record)
    {
        return RecordKind(record) switch
        {
            DatabaseRecordKind.Player or DatabaseRecordKind.Staff => Text(record.Nation),
            DatabaseRecordKind.Club => Text(record.Nation) ?? Text(record.Linked),
            DatabaseRecordKind.Competition or DatabaseRecordKind.NationalTeam => Text(record.Linked),
            _ => null
        };
    }

    private static string? RecordLeague(BridgeDatabaseRecordDto record)
    {
        return RecordKind(record) switch
        {
            DatabaseRecordKind.Player or DatabaseRecordKind.Staff => Text(record.League),
            DatabaseRecordKind.Club => Text(record.SecondaryLinked) ?? Text(record.League),
            _ => null
        };
    }

    private static string? RecordContinent(BridgeDatabaseRecordDto record)
    {
        return RecordKind(record) switch
        {
            DatabaseRecordKind.Nation => Text(record.Linked),
            DatabaseRecordKind.NationalTeam => Text(record.SecondaryLinked),
            _ => null
        };
    }

    private static string? Text(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static SearchInterestType? InterestType(BridgeInterestOptionDto? interest)
    {
        return PlayerSearchConstants.InterestTypeIds
            .Where(pair => pair.Value == interest?.Id)
            .Select(pair => (SearchInterestType?)pair.Key)
            .FirstOrDefault();
    }

    // The selected type's level; none when interest isn't considered.
    private static SearchInterestLevel? InterestLevel(BridgeInterestOptionDto? interest)
    {
        return PlayerSearchConstants.InterestLevels
            .Where(pair => pair.Value == interest?.Level)
            .Select(pair => (SearchInterestLevel?)pair.Key)
            .FirstOrDefault();
    }

    // The game's count of conditions, in brackets at the end of its "edit search" label, beyond those read one by one.
    private static int? OtherConditions(BridgeSearchFiltersDto filters)
    {
        var label = filters.State.EditFilterLabel ?? "";
        var open = label.LastIndexOf('(');
        var close = label.LastIndexOf(')');

        if (open < 0 || close <= open || !int.TryParse(label[(open + 1)..close], NumberStyles.None, CultureInfo.InvariantCulture, out var count))
        {
            return null;
        }

        var other = count - filters.QuickFilters.Count - filters.Rules.Count;
        return other > 0 ? other : null;
    }

    // The rule's value as the comparison uses it: a minimum, a maximum, a range, or the one value.
    private static string? RuleValue(BridgeSearchRuleDto rule)
    {
        var value = rule.Comparison?.Id switch
        {
            PlayerSearchConstants.AtLeast => rule.MinValue?.Name,
            PlayerSearchConstants.AtMost => rule.MaxValue?.Name,
            PlayerSearchConstants.Between or PlayerSearchConstants.NotBetween when rule is { MinValue.Name: { } min, MaxValue.Name: { } max }
                => $"{min}-{max}",
            _ => rule.Value?.Name
        };

        return Text(value) ?? Text(rule.ValueText);
    }

    private static List<PitchPosition> SearchPositions(List<BridgeFilterItemDto>? items)
    {
        return (items ?? [])
            .Where(item => item.Id is > 0)
            .Select(item => PositionMasks.ToPosition((ulong)item.Id!.Value)?.Position)
            .OfType<PitchPosition>()
            .ToList();
    }

    private static List<PositionFamiliarity> Familiarities(List<BridgeCompareFlagDto>? flags)
    {
        return (flags ?? [])
            .Select(flag => PlayerSearchConstants.Familiarities.Where(pair => pair.Value == flag.Id).Select(pair => (PositionFamiliarity?)pair.Key).FirstOrDefault())
            .OfType<PositionFamiliarity>()
            .ToList();
    }

    private static string? Display(BridgeSortedValueDto? value)
    {
        return value is null ? null : Text(value.DisplayValue);
    }

    // "7,76M € - 9,21M €", or the one value when both ends are the same (a fully scouted player).
    private static string? Range(string? min, string? max)
    {
        var low = Amount(min);
        var high = Amount(max);

        return low is null || high is null || low == high ? high ?? low : $"{low} - {high}";
    }

    // The game writes an amount it doesn't know as "-" or "-1 €"; no fee or wage is negative.
    private static string? Amount(string? text)
    {
        return Text(text) is { } amount && !amount.StartsWith(UnknownAmount, StringComparison.Ordinal) ? amount : null;
    }

    private static int? Days(BridgeSortedValueDto? days)
    {
        // Tables sort what they don't know last, as int.MaxValue; a player never scouted reads as NeverScoutedDays.
        return days?.SortValue is { } value && value is >= 0 and < NeverScoutedDays ? (int)value : null;
    }

    // Every status the game has for a player, each once: its full set, then what only the team screens' set holds. The
    // latter carries a placeholder (flag -1, "Invalid"), which isn't a status.
    private static List<BridgePlayerStatusDto> AllStatuses(List<BridgePlayerStatusDto>? all, List<BridgePlayerStatusDto>? team)
    {
        return (all ?? [])
            .Concat(team ?? [])
            .Where(status => status.StatusFlag is >= 0)
            .DistinctBy(status => status.StatusFlag)
            .ToList();
    }

    // A status flag by the game's English name, given as StatusKeysParameter.
    private static string? StatusKey(int? flag)
    {
        return flag is { } value
               && MapContext.Current?.Parameters.TryGetValue(StatusKeysParameter, out var names) == true
               && names is Dictionary<long, string> keys
            ? keys.GetValueOrDefault(value)
            : null;
    }

    // Long details come over several lines (every club watching a player): one line reads as well.
    private static string? OneLine(string? text)
    {
        return Text(text) is { } value
            ? string.Join(' ', value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            : null;
    }

    // The game dates what never happened 1 January 1900, written in the display language's format.
    private static bool IsBlankDate(string? date)
    {
        return string.IsNullOrWhiteSpace(date) || date.Contains(BlankYear, StringComparison.Ordinal);
    }

    private static string? Short(BridgeReportLineDto? line)
    {
        return line is null ? null : Range(line.ShortString, line.ShortString);
    }

    // The scout's sentences read on their own; the headings alone ("Versatility") don't say which way they cut.
    private static List<string> Sentences(List<BridgeProConDto> items)
    {
        return items
            .Select(item => Text(item.ProConsLongString) ?? Text(item.ProConsShortString))
            .OfType<string>()
            .ToList();
    }

    private static List<int> Ages(List<BridgeOptionDto> choices)
    {
        return choices.Where(choice => choice.PropertyValue is not null).Select(choice => (int)choice.PropertyValue!.Value).ToList();
    }

    private static List<TacticalPosition> GridPositions(List<BridgeFocusPositionDto> grid)
    {
        return grid
            .Where(box => box.Position is not null)
            .Select(box => PositionMasks.ToPosition(box.Position!.Value))
            .OfType<TacticalPosition>()
            .Distinct()
            .ToList();
    }

    private static string MenuLabel(BridgeMenuItemDto item)
    {
        return Text(item.Text) ?? Text(item.RowData?.Text) ?? string.Empty;
    }

    private static bool MenuRuns(BridgeMenuItemDto item)
    {
        var fields = MenuFields(item);
        return fields.Event == ScoutingConstants.DoActionEventId && fields.Disabled != true;
    }

    private static BridgeMenuItemDto MenuFields(BridgeMenuItemDto item)
    {
        return item.Text is null && item.RowData is not null ? item.RowData : item;
    }

    private static bool MenuWarns(BridgeMenuItemDto item)
    {
        return MenuFields(item).ActionData?.ConfirmationDialogData?.IsErrorDialog == true;
    }

    // The dialog's text on one line, as the game words it ("Ask the scouts to watch X? ... travel costs ...").
    private static string? MenuConfirmation(BridgeMenuItemDto item)
    {
        var description = Text(MenuFields(item).ActionData?.ConfirmationDialogData?.Description);

        return description is null
            ? null
            : string.Join(' ', description.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static List<TacticalPosition> TickedPositions(List<BridgeFocusPositionDto> grid, bool inPossession)
    {
        return grid
            .Where(box => box.Ticked is > 0 && (box.IsInPossession ?? true) == inPossession && box.Position is not null)
            .Select(box => PositionMasks.ToPosition(box.Position!.Value))
            .OfType<TacticalPosition>()
            .Distinct()
            .ToList();
    }

    private static string? RoleKey(ulong? mask)
    {
        return mask is { } role
               && MapContext.Current?.Parameters.TryGetValue(RoleKeysParameter, out var names) == true
               && names is IReadOnlyDictionary<ulong, string> keys
            ? keys.GetValueOrDefault(role)
            : null;
    }
}
