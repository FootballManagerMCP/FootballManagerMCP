namespace FootballManager.Infrastructure.Constants;

// The team's training, read as the game's training screens read it. The user's training calendar, with today's date as
// its "Date" context, lists this week and the next (WeeksOverview) and the session types by category. The training
// squad's players each have a PlayerTraining record; the individual training screen reads it with the user's manager as
// the "Judge" context, without which the additional focus and the out-of-possession role aren't answered.
internal static class TrainingConstants
{
    public const string Calendar = "TrainingCalendar";
    public const string DateContext = "Date";
    public const string Today = "game/CurrentGameDate";
    public const string Weeks = "WeeksOverview";

    // The current week's schedule; the weeks' own names come back empty.
    public const string Schedule = "TrainingScheduleName";
    public const string Categories = "TrainingSessionCategoryOnlyDropdown";
    public const string CategoryContext = "TrainingSessionCategory";
    public const string CategorySessions = "TrainingSessionsForCategoryDropdown";
    public const int FirstCategory = 0;
    public const int LastCategory = 4;

    // A day's events are its sessions and, on a match day, the fixture (a calendar event of its own).
    public const int MatchEvent = 1001;

    // A session's card: the day's date (a field id the game shares with other "Date"s), the session's place and type.
    public const string CardDate = "#1131498817";

    public const string BeginSessionEdit = "EventFmxBeginTrainingWeekEditFromCard";
    public const string SelectSession = "EventFmxSelectTrainingSession";
    public const string ConfirmSession = "EventFmxConfirmTrainingSessionFromCard";
    public const string CommitSessionEdit = "EventFmxCommitTrainingWeekFromCard";

    public const string Squad = "Team.TeamTraining.TrainingSquad";
    public const string JudgeContext = "Judge";
    public const string Manager = "Team.Manager";
    public const string PlayerTraining = "PlayerTraining";

    public const string Position = "PlayerTraining.TrainingPosition.Position";
    public const string PositionChoice = "PlayerTraining.TrainingPosition.NewPositionAndRoleRow.PropertyValue";
    public const string RoleId = "PlayerTraining.TrainingPosition.TargetRole.TacticsRoleID";
    public const string RoleName = "PlayerTraining.TrainingPosition.TargetRole.TacticsRoleName";
    public const string OutOfPossessionRole = "PlayerTraining.OOPTraining.IndividualOOPTrainingRoleRow";
    public const string Focus = "PlayerTraining.AdditionalFocus.IndividualTrainingTypeRow";
    public const string FocusCanEdit = "PlayerTraining.AdditionalFocus.CanEdit";
    public const string FocusHumanControlled = "PlayerTraining.AdditionalFocus.HumanControlled";
    public const string Intensity = "PlayerTraining.TrainingIntensityOrRest.IndividualTrainingIntensityRow";
    public const string IntensityAutomatic = "PlayerTraining.TrainingIntensityOrRest.IndividualTrainingIntensityAutoSelected";
    public const string IntensityCanEdit = "PlayerTraining.TrainingIntensityOrRest.CanEdit";
    public const string RestDays = "PlayerTraining.TrainingIntensityOrRest.RestDaysRemaining";
    public const string Rating = "PlayerTraining.TrainingRating";
    public const string Workload = "PlayerTraining.Workload";
    public const string FocusList = "PlayerTraining.IndividualTrainingAdditionalFocusList";
    public const string IntensityList = "PlayerTraining.IndividualTrainingIntensityList";

    public static readonly string[] PlayerProperties =
    [
        GameConstants.UniqueId, GameConstants.Name, Position, PositionChoice, RoleId, RoleName, OutOfPossessionRole, Focus,
        FocusCanEdit, FocusHumanControlled, Intensity, IntensityAutomatic, IntensityCanEdit, RestDays, Rating, Workload,
        FocusList, IntensityList
    ];

    // A training position "as in matches", the default; and the label of an unset choice (no focus, no role).
    public const long MatchPosition = 0;
    public const string Unset = "---";

    // A focus goes as the player's whole focus record with the new row in it, its fields in the game's order (the
    // dropdown's row alone does nothing); IndividualTrainingType is a name several ids share, so it goes by its id.
    public const string FocusEvent = "EventFmxPlayerTrainingFocusChanged";
    public const string FocusRecord = "AdditionalFocus";
    public const string FocusRowField = "IndividualTrainingTypeRow";
    public const string FocusRows = "IndividualTrainingAdditionalFocusList";

    public static readonly (string Field, string Source)[] FocusFields =
    [
        ("CanEdit", "CanEdit"), ("Available", "Available"), ("HumanControlled", "HumanControlled"),
        ("EventData", "EventData"), ("UnavailableReason", "UnavailableReason"), ("#1916366156", "IndividualTrainingType")
    ];

    public const string FocusChoicesField = "PossibleIndividualTrainingTypes";

    // An intensity goes as the dropdown's row; rest periods are rows under "no work", which the selector finds too. The
    // same dropdown sends a player on holiday (a submenu of 1 to 4 weeks), which isn't training: it's left out.
    public const string IntensityEvent = "EventFmxPlayerTrainingIntensityLevelChanged";
    public const string IntensityRows = "IndividualTrainingIntensityList";
    public const long Holidays = 19;

    public static string PlayerTrainingOf(int playerId)
    {
        return $"{Squad}[{GameConstants.UniqueId}={playerId}].{PlayerTraining}";
    }
}
