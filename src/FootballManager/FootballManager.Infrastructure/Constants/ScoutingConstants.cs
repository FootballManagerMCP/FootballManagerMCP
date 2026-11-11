using FootballManager.Domain.Scouting;

namespace FootballManager.Infrastructure.Constants;

// The game's paths to the user's scouting, from the user's manager (Human). Players outside the club read like the
// squad's, with the user's manager and team as contexts: the game then answers with what the club's scouting knows of
// them (ranges, the scouts' stars and grade), as its scouting screens do.
internal static class ScoutingConstants
{
    // Lists of players.
    public const string Recommendations = "ScoutingCentreActions.Players";
    public const string PersonSearch = "HumanPersonSearch.PersonList";
    public const string Focuses = "ScoutingInfo.RecruitmentFocusesList";
    public const string Scouts = "Club.Scouts";

    // Which list HumanPersonSearch holds, as the game's screens pass it: 0 is the user's shortlist, 1 the player search
    // (its results for the filters the search screen holds, which run over the game's whole database).
    public const string SearchType = "Type";
    public const int ShortlistSearch = 0;
    public const int PlayerSearch = 1;

    // The player search's state, on the user's manager with the search's type.
    public const string SearchStatus = "HumanPersonSearch.PersonListCountString";
    public const string SearchInterest = "HumanPersonSearch.PlayerInterestLevelString";
    public const string SearchAttributes = "HumanPersonSearch.AttributesSelectedText";

    // The scouting centre's counts, and the user's own lists; the latter only answer with IsPlayerRecruitment set.
    public const string CentreCounts = "ScoutingCentreActions.ScoutingCentreCounts";
    public const string ShortlistCount = "HumanShortlistPersonCount";
    public const string ScoutedPlayersCount = "HumanScoutedPlayersCount";
    public const string PlayerRecruitment = "IsPlayerRecruitment";

    // On a player.
    public const string ClubName = "Club.Name";
    public const string NationName = "Nation.Name";
    public const string Recommendation = "ScoutRecommendation";
    public const string GradeLabel = "ShortScoutRecommendation";
    public const string Grade = "ScoutRecommendationGrade";
    public const string AbilityStars = "ScoutCurrentAbilityStarRange";
    public const string PotentialStars = "ScoutPotentialAbilityStarRange";
    public const string MinValue = "ScoutedMinValue";
    public const string MaxValue = "ScoutedMaxValue";
    public const string MinWage = "ScoutedMinWage";
    public const string MaxWage = "ScoutedMaxWage";
    public const string BestPosition = "ScoutedBestPosition";
    public const string BestRole = "ScoutedBestRole";
    public const string BestOutOfPossessionRole = "ScoutedBestRoleOop";
    public const string StrongestArea = "ScoutedStrongestArea";
    public const string WeakestArea = "ScoutedWeakestArea";
    public const string Knowledge = "ScoutingKnowledgeLevel";
    public const string IsShortlisted = "IsShortlisted";
    public const string IsBeingScouted = "IsBeingScouted";
    public const string DaysSinceLastScouted = "DaysSinceLastScouted";

    public static readonly string[] PlayerBasics =
    [
        GameConstants.UniqueId, GameConstants.Name, GameConstants.Age, GameConstants.Positions, ClubName, NationName,
        Recommendation, GradeLabel, Grade, AbilityStars, PotentialStars, MinValue, MaxValue, MinWage, MaxWage,
        BestPosition, BestRole, BestOutOfPossessionRole, StrongestArea, WeakestArea, Knowledge, IsShortlisted,
        IsBeingScouted, DaysSinceLastScouted, GameConstants.Statuses, GameConstants.TeamStatuses
    ];

    // The club's latest report on a player. A report's parts are records; their fields read as paths too.
    private const string Report = "PlayerReportForContextTeam";
    public const string ReportDate = Report + ".ScoutedReportBasicInfo.DateCreated";
    public const string ReportScout = Report + ".ScoutedReportBasicInfo.ReportProvider.Name";
    public const string ReportAbility = Report + ".ScoutedCurrentAbilityInfo.ShortString";
    public const string ReportPotential = Report + ".ScoutedPotentialAbilityInfo.ShortString";
    public const string ReportProsCons = Report + ".ScoutedProConsList";
    public const string ReportTransfer = Report + ".ScoutedTransferInfo";

    public static readonly string[] PlayerReport =
    [
        ReportDate, ReportScout, ReportAbility, ReportPotential, ReportProsCons, ReportTransfer
    ];

    // On a recruitment focus. Its players' lists are picked by its id, which the plugin's selectors resolve.
    public const string FocusStatus = "FocusStatusString";
    public const string FocusComplete = "IsFocusComplete";
    public const string FocusStruggling = "IsFocusStruggling";
    public const string FocusAdvice = "RecruitmentFocusRecommendationString";
    public const string FocusAreas = "RecruitmentFocusAreasListString";
    public const string FocusScouts = "RecruitmentFocusScoutsListString";
    public const string FocusCreatedBy = "RecruitmentFocusCreatedByString";
    public const string FocusCreated = "RecruitmentFocusCreationDateShortString";
    public const string FocusMinAge = "MinAgeValue";
    public const string FocusMaxAge = "MaxAgeValue";
    public const string FocusMinAbility = "PlayerCurrentAbility";
    public const string FocusMinPotential = "PlayerPotentialAbility";
    public const string FocusPositions = "PositionsList";
    public const string RecommendedCount = "RecommendedPlayerPoolCount";
    public const string CurrentCount = "CurrentPlayerPoolCount";
    public const string NearMissCount = "NearMissPlayerPoolCount";

    public static readonly string[] FocusProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, FocusStatus, FocusComplete, FocusStruggling, FocusAdvice, FocusAreas,
        FocusScouts, FocusCreatedBy, FocusCreated, FocusMinAge, FocusMaxAge, FocusMinAbility, FocusMinPotential,
        FocusPositions, RecommendedCount, CurrentCount, NearMissCount
    ];

    public static string FocusPlayers(int focusId, FocusPool pool)
    {
        var list = pool switch
        {
            FocusPool.Current => "CurrentPlayerPool",
            FocusPool.NearMiss => "NearMissPlayerPool",
            _ => "RecommendedPlayerPool"
        };

        return $"{Focuses}[{GameConstants.UniqueId}={focusId}].{list}";
    }

    // The focus wizard works on a draft focus (id -1): its fields are set, then confirming creates a focus from it and
    // resets it, as cancelling does.
    public const string ScoutingInfo = "ScoutingInfo";

    // The season's scouting budget and what scouting has cost, from the user's manager, in the game's own unit (pounds:
    // a scouted wage's sort value is in pounds a week where the screen shows euros a month, 1.164 euros to the pound).
    public const string Budget = ScoutingInfo + ".ScoutingBudget";
    public const string CostsThisSeason = "Club.ScoutingCostsThisSeason";
    public const string CostsThisMonth = "Club.ScoutingCostsThisMonth";
    public const string Draft = "ScoutingInfo.NewRecruitmentFocus";
    public const string DraftAreaSelection = Draft + ".PossibleScoutingAreasDropdownSelectedOption";
    public const string AnyPosition = "AnyPosition";
    public const string MinAgeChoices = "MinAgeList";
    public const string MaxAgeChoices = "MaxAgeList";
    public const string AreaCount = "AreaCount";
    public const string IsValid = "IsValid";
    public const string DuplicateName = "DuplicateFocusName";
    public const string ConfirmFocusEvent = "EventFmxRecruitmentFocusEditConfirm";
    public const string CancelFocusEvent = "EventFmxRecruitmentFocusEditCancel";
    public const string SelectPositionEvent = "EventFmxSelectPosition";

    // Each of these steps a star threshold by one: true up, false down.
    public const string AbilityStepEvent = "EventFmxRecruitmentFocusCurrentAbilityUpdated";
    public const string PotentialStepEvent = "EventFmxRecruitmentFocusPotentialAbilityUpdated";

    // Picking an area adds it to the focus; the dropdown (on the team, for the user's manager) then shows nothing picked.
    // Its options are a tree: kinds, then groups (a continent, a country), then areas.
    public const string Areas = "Team.PossibleScoutingAreasDropdown";
    public const string AreasOnTeam = "PossibleScoutingAreasDropdown";
    public const string AreaIdField = "DatabaseReferenceField";

    // The focus list is the scouting screen's, filtered as the screen shows it: 0 every active focus (the default), 1
    // those in progress, 2 those completed. Writes that look for a focus show them all while they run.
    public const string FocusListType = "RecruitmentFocusesListType";
    public const string FocusListTypeFromHuman = ScoutingInfo + "." + FocusListType;
    public const int AllFocusesListType = 0;

    // Removing a focus goes through the scouting screen's tables, which the screen refreshes as it opens (0: the
    // first table); until then the game may not find the focus. The removal takes the focus off the scouts (the count
    // drops) but leaves it in the screen's list until it's deleted, which only clears a removed focus.
    public const string RefreshTablesEvent = "EventFmxRecruitmentFocusChangeResultsTable";
    public const string RemoveFocusEvent = "EventFmxRecruitmentFocusRemove";
    public const string DeleteFocusEvent = "EventFmxRecruitmentFocusDelete";
    public const string FocusCountFromHuman = ScoutingInfo + ".RecruitmentFocuses.Count";

    // A player's menu, as the game builds it for its screens: game.Actions lists what the "Records" offer (by action
    // id); game.Action is one of them, its Items the menu's entries, each with the event it sends (EventFmxDoAction)
    // and the payload the game built for it (ActionData).
    public const string Actions = "Actions";
    public const string Action = "Action";
    public const string ActionId = "ID";
    public const string Records = "Records";
    public const string MenuItems = "Items";
    public const string DoActionEvent = "EventFmxDoAction";
    public const int DoActionEventId = 44;

    // FM.GamePlugin.ActionID values.
    public const uint AssignScoutAction = 1363818831;
    public const uint AddToShortlistAction = 1097488485;
    public const uint RemoveFromShortlistAction = 1500280650;

    // "Offer trial" > 1, 2, 3 or 4 weeks; once offered, the player's menu has the withdrawal instead
    // (PersonWithdrawPlayerTransferOffer, the same entry as for a transfer offer).
    public const uint OfferTrialAction = 1749513580;
    public const uint WithdrawOfferAction = 1951493231;

    // "End trial" (PersonTerminateTrial), a single entry, on a player on trial at the club.
    public const uint EndTrialAction = 1299399024;

    public static uint ActionIdOf(PlayerMenuAction action)
    {
        return action switch
        {
            PlayerMenuAction.AddToShortlist => AddToShortlistAction,
            PlayerMenuAction.RemoveFromShortlist => RemoveFromShortlistAction,
            PlayerMenuAction.OfferTrial => OfferTrialAction,
            PlayerMenuAction.WithdrawOffer => WithdrawOfferAction,
            PlayerMenuAction.EndTrial => EndTrialAction,
            _ => AssignScoutAction
        };
    }

    // On a scout.
    public const string Job = "JobString";
    public const string JudgingAbility = "JudgingPlayerAbility";
    public const string JudgingPotential = "JudgingPlayerPotential";

    public static readonly string[] ScoutProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, Job, JudgingAbility, JudgingPotential
    ];
}
