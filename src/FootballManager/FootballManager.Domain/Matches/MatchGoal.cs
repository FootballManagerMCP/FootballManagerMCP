namespace FootballManager.Domain.Matches;

public class MatchGoal
{
    /// <summary>As the game writes it, e.g. "21'" or "90+2'".</summary>
    public required string Minute { get; set; }

    /// <summary>True when the user's team scored it (an own goal counts for the side it was scored for).</summary>
    public bool IsOurs { get; set; }

    public GoalKind Kind { get; set; }

    /// <summary>An own goal's scorer is the player who put it into his own net.</summary>
    public string? Scorer { get; set; }

    /// <summary>None for a penalty or an own goal, nor for a goal no one set up.</summary>
    public string? Assister { get; set; }
}
