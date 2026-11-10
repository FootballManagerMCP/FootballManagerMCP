using FootballManager.Domain.Players;

namespace FootballManager.Infrastructure.Constants;

// The game's own property paths, as its UI binds them. Names come from the game's property registry (/schema and
// /catalog on the plugin list them all).
internal static class GameConstants
{
    public const string HumanRoot = "Human";
    public const string GameRoot = "game";

    public const string Team = "Team";
    public const string TeamName = "Team.Name";
    public const string TeamId = "Team.UniqueId";

    public const string UniqueId = "UniqueId";
    public const string Name = "Name";
    public const string Age = "Age";
    public const string Positions = "PositionCombinedString";
    public const string BestRole = "BestRole";
    public const string Personality = "Personality";
    public const string Morale = "MoraleString";
    public const string Condition = "Condition";
    public const string Sharpness = "Sharpness";
    public const string IsInjured = "IsInjured";
    public const string AverageRating = "AverageRating";
    public const string CurrentAbility = "PlayerCurrentAbility";
    public const string PotentialAbility = "PlayerPotentialAbility";

    // A player's status icons, each with a flag of StatusFlagsEnum: all of them, and the team screens' set, which can
    // hold a few the other lacks (scouted or analysed by the user's club). The recruitment screens' set is a subset.
    public const string Statuses = "PlayerStatus";
    public const string TeamStatuses = "PlayerStatusFlagsForTeam";
    public const string StatusFlagsEnum = "FM.UI.PlayerStatusFlag";

    public static readonly string[] PlayerBasics =
    [
        UniqueId, Name, Age, Positions, BestRole, Personality, Morale, Condition, Sharpness, IsInjured, AverageRating,
        Statuses, TeamStatuses
    ];

    // This season's numbers, all competitive matches for the club together.
    public const string Appearances = "Appearances";
    public const string Minutes = "Minutes";
    public const string Goals = "Goals";
    public const string Assists = "Assists";
    public const string ExpectedGoals = "ExpectedGoals";
    public const string NonPenaltyExpectedGoals = "NonPenaltyExpectedGoals";
    public const string ExpectedGoalsOverperformance = "ExpectedGoalsOverperformance";
    public const string ExpectedGoalsPerShot = "ExpectedGoalsPerShot";
    public const string ExpectedAssists = "ExpectedAssists";
    public const string ShotsPer90 = "ShotsPer90";
    public const string ShotsOnTarget = "ShotsOnTarget";
    public const string KeyPasses = "KeyPasses";
    public const string ChancesCreated = "ChancesCreated";
    public const string PassCompletionPercentage = "PassCompletionPercentage";

    public static readonly string[] PlayerSeasonStats =
    [
        Appearances, Minutes, Goals, Assists, ExpectedGoals, NonPenaltyExpectedGoals, ExpectedGoalsOverperformance,
        ExpectedGoalsPerShot, ExpectedAssists, ShotsPer90, ShotsOnTarget, KeyPasses, ChancesCreated, PassCompletionPercentage
    ];

    // An attribute is a small record: its range bounds are equal once the player is fully known.
    public static string AttributeMin(AttributeDefinition attribute)
    {
        return $"Attribute{attribute.Name}.AttributeMinValue";
    }

    public static string AttributeMax(AttributeDefinition attribute)
    {
        return $"Attribute{attribute.Name}.AttributeMaxValue";
    }
}
