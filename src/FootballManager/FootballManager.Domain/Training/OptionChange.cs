namespace FootballManager.Domain.Training;

/// <summary>A training setting's change: what it was, what it's set to, and once sent whether the game shows it.</summary>
public class OptionChange
{
    public TrainingOption? From { get; set; }
    public TrainingOption? To { get; set; }

    /// <summary>Set once applied: the setting read back from the game equals To.</summary>
    public bool? Done { get; set; }
}
