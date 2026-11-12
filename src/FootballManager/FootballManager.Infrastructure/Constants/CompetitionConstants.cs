using FootballManager.Domain.Competitions;

namespace FootballManager.Infrastructure.Constants;

// A competition's stats screen, from the user's manager (Human). Its player stats are one table per stat, picked with the
// stat's id as the Type context and a stage (-1 for all of them, as the screen's default "All stages"); each table
// keeps the competition's best 100 in that stat. The leagues reachable are the user's own, and those of the nations
// the game plays out on the user's continent (a league split in two, like Wales' second tier, through its children).
internal static class CompetitionConstants
{
    public const string UserLeague = TeamConstants.OwnTeam + ".Division";
    public const string UserTeamId = TeamConstants.OwnTeam + "." + GameConstants.UniqueId;
    public const string Nations = "Club.Nation.Continent.PlayableNations";
    public const string Leagues = "PlayableLeagues";
    public const string Children = "ChildCompList";

    // Lists of nations and their leagues are per gender; 0 is the men's game.
    public const string GenderContext = "Gender";
    public const int Men = 0;

    // On a competition.
    public const string NationName = "Nation.Name";
    public const string Reputation = "Reputation";
    public const string ChildCount = "ChildCompListCount";

    public static readonly string[] CompetitionProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, Reputation, NationName, ChildCount
    ];

    // The stats screen.
    public const string StatContext = "Type";
    public const string StageContext = "Stage";
    public const int AllStages = -1;
    public const string Table = "PlayerStats";
    public const string Rows = Table + ".Players";
    public const string Label = "StatValueHeaderLong";

    // On a table's row: the player and his team are records; the stat's value as a number, and the rank as the game
    // writes it ("1st", or "=" when tied with the row above). A list's rows hold their player's properties; reading
    // several tables at once leaves some of them unanswered, so tables are read one at a time.
    public const string RowPlayerId = "Player." + GameConstants.UniqueId;
    public const string RowName = "Player." + GameConstants.Name;
    public const string RowAge = "Player." + GameConstants.Age;
    public const string RowPositions = "Player.PositionCombined";
    public const string RowMinutes = "Player." + GameConstants.Minutes;
    public const string RowTeamId = "Team." + GameConstants.UniqueId;
    public const string RowTeam = "Team." + GameConstants.Name;
    public const string RowValue = "StatValueFloat";
    public const string RowRank = "Position";

    public static readonly string[] RowProperties =
    [
        RowPlayerId, RowName, RowAge, RowPositions, RowMinutes, RowTeamId, RowTeam, RowValue, RowRank
    ];

    // On a player: his contract with his club, and what he'd cost.
    public const string ContractEndDate = "FullContract.EndDate";
    public const string ContractHasEndDate = "FullContract.HasEndDate";
    public const string ContractType = "FullContract.ContractType";
    // The status with the contract's kind ("Amateur contract", "Part-time contract"); the type is its term ("Monthly",
    // or a fixed-term contract).
    public const string ContractStatus = "FullContract.ContractStatusWithType";
    public const string Wage = "ContextContractWeeklyWage";
    public const string TransferValue = "TransferValueRange";
    public const string TransferStatus = "TransferStatusDetails";

    public static readonly string[] ContractProperties =
    [
        ContractEndDate, ContractHasEndDate, ContractType, ContractStatus, Wage, TransferValue, TransferStatus
    ];

    public static string Row(int index)
    {
        return TeamConstants.Item(Rows, index) + ".Player";
    }

    // The game's ids for the stats (a native enum with no managed names): found by reading each id's table header.
    public static readonly IReadOnlyDictionary<CompetitionStat, int> StatIds = new Dictionary<CompetitionStat, int>
    {
        [CompetitionStat.Appearances] = 1,
        [CompetitionStat.Starts] = 45,
        [CompetitionStat.Minutes] = 56,
        [CompetitionStat.AverageRating] = 27,
        [CompetitionStat.PlayerOfTheMatch] = 12,
        [CompetitionStat.MatchesWon] = 8,
        [CompetitionStat.MatchesLost] = 9,
        [CompetitionStat.YellowCards] = 10,
        [CompetitionStat.RedCards] = 11,
        [CompetitionStat.Offsides] = 21,
        [CompetitionStat.OffsidesPer90] = 120,
        [CompetitionStat.DistanceCovered] = 17,
        [CompetitionStat.DistanceCoveredPer90] = 18,
        [CompetitionStat.Sprints] = 127,
        [CompetitionStat.SprintsPer90] = 132,

        [CompetitionStat.Goals] = 2,
        [CompetitionStat.GoalsPer90] = 72,
        [CompetitionStat.MinutesPerGoal] = 19,
        [CompetitionStat.PenaltiesScored] = 4,
        [CompetitionStat.PenaltyConversionPercentage] = 130,
        [CompetitionStat.FreeKickGoals] = 65,
        [CompetitionStat.FreeKickGoalsPer90] = 122,
        [CompetitionStat.Shots] = 46,
        [CompetitionStat.ShotsPer90] = 76,
        [CompetitionStat.ShotsOnTarget] = 47,
        [CompetitionStat.ShotsOnTargetPer90] = 77,
        [CompetitionStat.ShotsOnTargetPercentage] = 16,
        [CompetitionStat.ConversionPercentage] = 55,
        [CompetitionStat.ExpectedGoals] = 68,
        [CompetitionStat.ExpectedGoalsPer90] = 74,
        [CompetitionStat.NonPenaltyExpectedGoals] = 109,
        [CompetitionStat.NonPenaltyExpectedGoalsPer90] = 110,
        [CompetitionStat.ExpectedGoalsPerShot] = 78,
        [CompetitionStat.ExpectedGoalsOverperformance] = 107,
        [CompetitionStat.ExpectedGoalsUnderperformance] = 108,

        [CompetitionStat.Assists] = 5,
        [CompetitionStat.AssistsPer90] = 73,
        [CompetitionStat.ExpectedAssists] = 69,
        [CompetitionStat.ExpectedAssistsPer90] = 75,
        [CompetitionStat.OpenPlayExpectedAssists] = 111,
        [CompetitionStat.OpenPlayExpectedAssistsPer90] = 135,
        [CompetitionStat.SetPieceExpectedAssists] = 141,
        [CompetitionStat.KeyPasses] = 23,
        [CompetitionStat.KeyPassesPer90] = 81,
        [CompetitionStat.OpenPlayKeyPasses] = 131,
        [CompetitionStat.OpenPlayKeyPassesPer90] = 133,
        [CompetitionStat.ChancesCreated] = 54,
        [CompetitionStat.ChancesCreatedPer90] = 113,
        [CompetitionStat.Dribbles] = 62,
        [CompetitionStat.DribblesPer90] = 15,
        [CompetitionStat.CrossesAttempted] = 82,
        [CompetitionStat.CrossesAttemptedPer90] = 83,
        [CompetitionStat.CrossesCompleted] = 102,
        [CompetitionStat.CrossesCompletedPer90] = 123,
        [CompetitionStat.CrossCompletionPercentage] = 22,
        [CompetitionStat.OpenPlayCrossesAttemptedPer90] = 136,
        [CompetitionStat.OpenPlayCrossesCompletedPer90] = 137,
        [CompetitionStat.SetPieceCrossesAttempted] = 140,

        [CompetitionStat.PassesAttempted] = 79,
        [CompetitionStat.PassesAttemptedPer90] = 80,
        [CompetitionStat.PassesCompleted] = 101,
        [CompetitionStat.PassesCompletedPer90] = 124,
        [CompetitionStat.PassCompletionPercentage] = 13,
        [CompetitionStat.ProgressivePasses] = 126,
        [CompetitionStat.ProgressivePassesPer90] = 134,

        [CompetitionStat.TacklesAttempted] = 89,
        [CompetitionStat.TacklesAttemptedPer90] = 90,
        [CompetitionStat.TacklesWon] = 63,
        [CompetitionStat.TacklesWonPer90] = 14,
        [CompetitionStat.TackleSuccessPercentage] = 91,
        [CompetitionStat.KeyTackles] = 24,
        [CompetitionStat.KeyTacklesPer90] = 125,
        [CompetitionStat.Interceptions] = 26,
        [CompetitionStat.InterceptionsPer90] = 92,
        [CompetitionStat.PossessionWon] = 67,
        [CompetitionStat.PossessionWonPer90] = 93,
        [CompetitionStat.PossessionLost] = 66,
        [CompetitionStat.PossessionLostPer90] = 88,
        [CompetitionStat.Blocks] = 64,
        [CompetitionStat.BlocksPer90] = 94,
        [CompetitionStat.ShotsBlocked] = 53,
        [CompetitionStat.ShotsBlockedPer90] = 121,
        [CompetitionStat.Clearances] = 70,
        [CompetitionStat.ClearancesPer90] = 96,
        [CompetitionStat.PressuresAttempted] = 128,
        [CompetitionStat.PressuresCompleted] = 129,
        [CompetitionStat.Fouls] = 84,
        [CompetitionStat.FoulsPer90] = 86,
        [CompetitionStat.FoulsWon] = 85,
        [CompetitionStat.FoulsWonPer90] = 87,
        [CompetitionStat.Errors] = 28,
        [CompetitionStat.ErrorsLeadingToGoal] = 29,

        [CompetitionStat.AerialDuels] = 51,
        [CompetitionStat.AerialDuelsPer90] = 97,
        [CompetitionStat.HeadersWon] = 52,
        [CompetitionStat.HeadersWonPer90] = 95,
        [CompetitionStat.HeaderWinPercentage] = 30,
        [CompetitionStat.HeadersLost] = 138,
        [CompetitionStat.HeadersLostPer90] = 139,
        [CompetitionStat.KeyHeaders] = 25,
        [CompetitionStat.KeyHeadersPer90] = 112,

        [CompetitionStat.GoalsConceded] = 3,
        [CompetitionStat.GoalsConcededPer90] = 35,
        [CompetitionStat.CleanSheets] = 20,
        [CompetitionStat.CleanSheetsPer90] = 119,
        [CompetitionStat.Saves] = 103,
        [CompetitionStat.SavesPer90] = 100,
        [CompetitionStat.SavesHeld] = 48,
        [CompetitionStat.SavesParried] = 49,
        [CompetitionStat.SavesTipped] = 50,
        [CompetitionStat.SavePercentage] = 71,
        [CompetitionStat.ExpectedSavePercentage] = 105,
        [CompetitionStat.ExpectedSavesPer90] = 104,
        [CompetitionStat.ExpectedGoalsPrevented] = 106,
        [CompetitionStat.ExpectedGoalsPreventedPer90] = 118,
        [CompetitionStat.ExpectedGoalsConcededPer90] = 98,
        [CompetitionStat.PenaltiesFaced] = 59,
        [CompetitionStat.PenaltiesSaved] = 58,
        [CompetitionStat.PenaltySavePercentage] = 99
    };
}
