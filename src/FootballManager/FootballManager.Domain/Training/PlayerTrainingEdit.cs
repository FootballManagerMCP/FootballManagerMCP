namespace FootballManager.Domain.Training;

/// <summary>A change to one player's training; what's left out stays as it is.</summary>
public class PlayerTrainingEdit
{
    public int PlayerId { get; set; }

    /// <summary>An additional focus's id (TrainingOverview.FocusOptions), -1 for none.</summary>
    public long? Focus { get; set; }

    /// <summary>An intensity's or rest period's id (TrainingOverview.IntensityOptions).</summary>
    public long? Intensity { get; set; }
}
