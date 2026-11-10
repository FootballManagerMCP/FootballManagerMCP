namespace FootballManager.Domain.Scouting;

/// <summary>One of the club's scouts or recruitment staff.</summary>
public class Scout
{
    /// <summary>FM's unique id for the person.</summary>
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>The job as the game names it, e.g. "Scout".</summary>
    public string? Job { get; set; }

    /// <summary>Judging player ability, 1-20.</summary>
    public int? JudgingAbility { get; set; }

    /// <summary>Judging player potential, 1-20.</summary>
    public int? JudgingPotential { get; set; }
}
