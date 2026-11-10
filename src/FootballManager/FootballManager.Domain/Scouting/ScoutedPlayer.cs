using FootballManager.Domain.Players;
using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Scouting;

/// <summary>
/// A player outside the club as the user's scouts know him: only what the game shows the user, so ranges where
/// knowledge is partial. Text values (positions, verdicts, areas, value and wage) are in the game's display language.
/// </summary>
public class ScoutedPlayer
{
    /// <summary>FM's unique id for the person, stable for the lifetime of the save.</summary>
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }
    public string? Club { get; set; }
    public string? Nation { get; set; }

    /// <summary>Positions as FM abbreviates them, e.g. "D (C)" or "M/MO (G)".</summary>
    public string? Positions { get; set; }

    /// <summary>The position the scouts rate him best in.</summary>
    public TacticalPosition? BestPosition { get; set; }

    /// <summary>The scouts' best role for him with the ball, as the game's English key (e.g. "BallPlayingCentreBack").</summary>
    public string? BestRole { get; set; }

    /// <summary>The scouts' best role for him without the ball, as the game's English key.</summary>
    public string? BestOutOfPossessionRole { get; set; }

    /// <summary>The scouts' grade, 0 ("needs watching") to 12 ("priority"); see <see cref="GradeLabel"/>.</summary>
    public int? Grade { get; set; }

    /// <summary>The grade as the game names it, e.g. "Excellent".</summary>
    public string? GradeLabel { get; set; }

    /// <summary>The scouts' verdict, e.g. "Would be an excellent signing."</summary>
    public string? Recommendation { get; set; }

    /// <summary>The scouts' current ability stars on the game's 0-20 scale (20 is five stars), relative to the user's squad.</summary>
    public Rating? AbilityStars { get; set; }

    /// <summary>The scouts' potential ability stars on the same scale.</summary>
    public Rating? PotentialStars { get; set; }

    /// <summary>How well the scouts know him, out of <see cref="KnowledgeMax"/>; ranges narrow as it grows.</summary>
    public int? KnowledgeLevel { get; set; }
    public int? KnowledgeMax { get; set; }

    /// <summary>The knowledge level as the game names it.</summary>
    public string? Knowledge { get; set; }

    /// <summary>The scouts' estimate of his transfer value, e.g. "7,76M € - 9,21M €".</summary>
    public string? Value { get; set; }

    /// <summary>The scouts' estimate of his wage demands.</summary>
    public string? Wage { get; set; }

    public string? StrongestArea { get; set; }
    public string? WeakestArea { get; set; }
    public bool IsShortlisted { get; set; }
    public bool IsBeingScouted { get; set; }
    public int? DaysSinceLastScouted { get; set; }

    /// <summary>The statuses the game shows for him: a transfer agreed, listed for transfer or loan, injured, ...</summary>
    public List<PlayerStatus> Statuses { get; set; } = [];

    /// <summary>Only filled when asked for; ranges while knowledge is partial.</summary>
    public List<PlayerAttribute> Attributes { get; set; } = [];

    /// <summary>The latest scout report, only filled when asked for.</summary>
    public ScoutReport? Report { get; set; }

    /// <summary>Hidden. Only requested when the user agreed to see hidden data; null when the game masks it.</summary>
    public int? CurrentAbility { get; set; }

    /// <summary>Hidden. Only requested when the user agreed to see hidden data; null when the game masks it.</summary>
    public int? PotentialAbility { get; set; }

    public bool HasHiddenValues => CurrentAbility is not null
        || PotentialAbility is not null
        || Attributes.Any(a => a.Category == AttributeCategory.Hidden && a.IsKnown);
}
