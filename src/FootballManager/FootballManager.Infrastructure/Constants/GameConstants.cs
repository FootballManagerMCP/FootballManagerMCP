using FootballManager.Domain.Attributes;

namespace FootballManager.Infrastructure.Constants;

// The game's own property paths, as its UI binds them. Names come from the game's property registry (/schema and
// /catalog on the plugin list them all).
internal static class GameConstants
{
    public const string HumanRoot = "Human";

    public const string TeamName = "Team.Name";

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

    public static readonly string[] PlayerBasics =
    [
        UniqueId, Name, Age, Positions, BestRole, Personality, Morale, Condition, Sharpness, IsInjured, AverageRating
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
