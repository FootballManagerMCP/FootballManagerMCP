using System.Globalization;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using FootballManager.Domain.Tactics.Effectiveness;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Dtos.Tactics;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;
using FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;
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
            .Map(dest => dest.Attributes, src => AttributeBoundsMapper.ToAttributes(src.AttributeBounds))
            // Appearances always comes back when the season's numbers were asked for.
            .Map(dest => dest.SeasonStats, src => src.Appearances == null ? null : src);

        config.NewConfig<BridgePlayerDto, PlayerSeasonStats>();

        RegisterTactics(config);
        RegisterMatches(config);
        RegisterScouting(config);
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
            .Ignore(dest => dest.Scouts, dest => dest.Focuses);
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
        var fields = item.Text is null && item.RowData is not null ? item.RowData : item;
        return fields.Event == ScoutingConstants.DoActionEventId && fields.Disabled != true;
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
