namespace FootballManager.Domain.Training;

/// <summary>A choice the game's training screens offer (an additional focus, an intensity, a session): its id and label.</summary>
public class TrainingOption
{
    public long Id { get; set; }

    /// <summary>In the game's display language.</summary>
    public string Name { get; set; } = string.Empty;
}
