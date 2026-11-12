namespace FootballManager.Api.Responses.Training;

public class PlayerTrainingChangeResponse
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OptionChangeResponse? Focus { get; set; }
    public OptionChangeResponse? Intensity { get; set; }
}
