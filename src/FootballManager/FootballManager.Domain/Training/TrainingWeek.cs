namespace FootballManager.Domain.Training;

/// <summary>A week of the training schedule, Monday to Sunday.</summary>
public class TrainingWeek
{
    /// <summary>The Monday, yyyy-MM-dd.</summary>
    public string? StartDate { get; set; }

    public List<TrainingDay> Days { get; set; } = [];
}
