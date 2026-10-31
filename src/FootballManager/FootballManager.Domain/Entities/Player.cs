using FootballManager.Domain.Attributes;

namespace FootballManager.Domain.Entities;

/// <summary>
/// A player as the user's club sees them. Text values (positions, role, morale, personality) are in the game's
/// display language.
/// </summary>
public class Player
{
    /// <summary>FM's unique id for the person, stable for the lifetime of the save.</summary>
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }

    /// <summary>Positions as FM abbreviates them, e.g. "D (C)" or "M/MO (G)".</summary>
    public string? Positions { get; set; }
    public string? BestRole { get; set; }
    public string? Personality { get; set; }
    public string? Morale { get; set; }

    /// <summary>Physical condition in percent; null while the player is injured.</summary>
    public int? Condition { get; set; }

    /// <summary>Match sharpness in percent.</summary>
    public int? MatchSharpness { get; set; }
    public bool IsInjured { get; set; }

    /// <summary>Average match rating this season; null without appearances.</summary>
    public double? AverageRating { get; set; }

    public List<PlayerAttribute> Attributes { get; set; } = [];

    /// <summary>Hidden. Only requested when the user agreed to see hidden data; null when the game masks it.</summary>
    public int? CurrentAbility { get; set; }

    /// <summary>Hidden. Only requested when the user agreed to see hidden data; null when the game masks it.</summary>
    public int? PotentialAbility { get; set; }

    public bool HasHiddenValues => CurrentAbility is not null
        || PotentialAbility is not null
        || Attributes.Any(a => a.Category == AttributeCategory.Hidden && a.IsKnown);
}
