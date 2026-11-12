namespace FootballManager.Api.Requests.Training;

public class PlayerTrainingRequest
{
    /// <summary>The player's FM id, as get_training lists him.</summary>
    public int PlayerId { get; set; }

    /// <summary>An additional focus's id from get_training's focusOptions, -1 for none.</summary>
    public long? Focus { get; set; }

    /// <summary>An intensity's or rest period's id from get_training's intensityOptions.</summary>
    public long? Intensity { get; set; }
}
