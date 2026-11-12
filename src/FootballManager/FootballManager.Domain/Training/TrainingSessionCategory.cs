namespace FootballManager.Domain.Training;

/// <summary>A category of training sessions (tactical, match preparation, physical...) and the sessions it holds.</summary>
public class TrainingSessionCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TrainingOption> Sessions { get; set; } = [];
}
