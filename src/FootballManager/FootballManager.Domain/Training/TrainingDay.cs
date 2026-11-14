namespace FootballManager.Domain.Training;

/// <summary>A day of the training schedule.</summary>
public class TrainingDay
{
    /// <summary>yyyy-MM-dd.</summary>
    public string Date { get; set; } = string.Empty;

    public List<TrainingSession> Sessions { get; set; } = [];

    /// <summary>Where the team plays that day ("home" or "away", in the game's language), when it has a match.</summary>
    public string? Match { get; set; }
}
