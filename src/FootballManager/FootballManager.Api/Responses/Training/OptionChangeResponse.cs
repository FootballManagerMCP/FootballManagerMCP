namespace FootballManager.Api.Responses.Training;

public class OptionChangeResponse
{
    public TrainingOptionResponse? From { get; set; }
    public TrainingOptionResponse? To { get; set; }

    /// <summary>Once applied: whether the game shows the change.</summary>
    public bool? Done { get; set; }
}
