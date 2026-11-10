using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Scouting;

/// <summary>
/// A recruitment focus: a standing brief for the club's scouts (where to look, which positions, ages and level), with
/// the players it has turned up. Text values are in the game's display language.
/// </summary>
public class RecruitmentFocus
{
    /// <summary>The focus's id in the game, stable while it exists.</summary>
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// The game gave this id to a newer focus after this one was removed, but still shows the removed focus's settings
    /// and players under it; the newer focus can't be read until the game refreshes its list.
    /// </summary>
    public bool ShowsRemovedFocus { get; set; }

    /// <summary>As the game writes it, e.g. "In progress".</summary>
    public string? Status { get; set; }
    public bool IsComplete { get; set; }

    /// <summary>The game finds it isn't turning up enough recommended players.</summary>
    public bool IsStruggling { get; set; }

    /// <summary>The scouts' comment on how it's going.</summary>
    public string? Advice { get; set; }

    /// <summary>Where the scouts look, e.g. "Europe" or "Spain".</summary>
    public string? Areas { get; set; }

    /// <summary>The scouts assigned, as the game lists them.</summary>
    public string? Scouts { get; set; }

    /// <summary>As the game writes it, e.g. "Created by: Recruitment analyst".</summary>
    public string? CreatedBy { get; set; }

    /// <summary>As the game writes dates, e.g. "21/10/2026".</summary>
    public string? Created { get; set; }

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    /// <summary>The current ability stars a player needs, on the game's 0-20 scale (20 is five stars).</summary>
    public Rating? MinAbilityStars { get; set; }

    /// <summary>The potential ability stars a player needs, on the same scale.</summary>
    public Rating? MinPotentialStars { get; set; }

    /// <summary>The positions it looks for with the ball; empty means any position.</summary>
    public List<TacticalPosition> Positions { get; set; } = [];

    /// <summary>The positions it looks for without the ball, if any are set.</summary>
    public List<TacticalPosition> OutOfPossessionPositions { get; set; } = [];

    public int? RecommendedPlayers { get; set; }
    public int? CurrentPlayers { get; set; }
    public int? NearMissPlayers { get; set; }
}
