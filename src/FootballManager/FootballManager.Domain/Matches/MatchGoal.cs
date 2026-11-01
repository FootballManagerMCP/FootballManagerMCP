namespace FootballManager.Domain.Matches;

public class MatchGoal
{
    /// <summary>As the game writes it, e.g. "21'" or "90+2'".</summary>
    public required string Minute { get; set; }

    /// <summary>True when the user's team scored it.</summary>
    public bool IsOurs { get; set; }

    /// <summary>Names are only known for the user's own players.</summary>
    public string? Scorer { get; set; }
    public string? Assister { get; set; }
}
