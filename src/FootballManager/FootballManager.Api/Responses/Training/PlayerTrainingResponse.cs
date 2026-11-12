namespace FootballManager.Api.Responses.Training;

public class PlayerTrainingResponse
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? RoleKey { get; set; }
    public string? RoleName { get; set; }
    public bool FollowsMatchRole { get; set; }
    public string? OutOfPossessionRole { get; set; }
    public TrainingOptionResponse? Focus { get; set; }
    public TrainingOptionResponse? Intensity { get; set; }

    /// <summary>True when the game picks the intensity; left out otherwise.</summary>
    public bool? IntensityIsAutomatic { get; set; }

    public int? RestDays { get; set; }

    public double? Rating { get; set; }
    public string? Workload { get; set; }

    /// <summary>True when the staff set his focus, so it can't be set here; left out otherwise.</summary>
    public bool? FocusLocked { get; set; }

    /// <summary>True when the medical staff set his intensity (he's injured); left out otherwise.</summary>
    public bool? IntensityLocked { get; set; }
}
