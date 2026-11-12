namespace FootballManager.Domain.Training;

/// <summary>A set_training plan, or its outcome once applied.</summary>
public class TrainingChanges
{
    public bool Applied { get; set; }
    public List<PlayerTrainingChange> Players { get; set; } = [];
    public List<SessionChange> Sessions { get; set; } = [];

    /// <summary>What didn't go as planned, e.g. the game stopping partway.</summary>
    public List<string> Notes { get; set; } = [];
}
