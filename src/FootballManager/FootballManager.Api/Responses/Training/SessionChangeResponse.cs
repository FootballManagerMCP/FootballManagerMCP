namespace FootballManager.Api.Responses.Training;

public class SessionChangeResponse
{
    public string Date { get; set; } = string.Empty;
    public int Slot { get; set; }
    public TrainingOptionResponse? From { get; set; }
    public TrainingOptionResponse? To { get; set; }
    public bool? Done { get; set; }
}
