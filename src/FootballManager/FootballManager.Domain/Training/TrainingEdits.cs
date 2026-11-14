namespace FootballManager.Domain.Training;

/// <summary>What to change in the team's training: players' own training, and sessions of the schedule.</summary>
public class TrainingEdits
{
    public List<PlayerTrainingEdit> Players { get; set; } = [];
    public List<SessionEdit> Sessions { get; set; } = [];
}
