namespace FootballManager.Domain.Training;

/// <summary>A player's planned or applied training change.</summary>
public class PlayerTrainingChange
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OptionChange? Focus { get; set; }
    public OptionChange? Intensity { get; set; }
}
