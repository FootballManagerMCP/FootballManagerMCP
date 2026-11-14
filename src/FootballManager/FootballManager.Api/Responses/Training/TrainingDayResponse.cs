namespace FootballManager.Api.Responses.Training;

public class TrainingDayResponse
{
    public string Date { get; set; } = string.Empty;
    public List<TrainingSessionResponse> Sessions { get; set; } = [];
    public string? Match { get; set; }
}
