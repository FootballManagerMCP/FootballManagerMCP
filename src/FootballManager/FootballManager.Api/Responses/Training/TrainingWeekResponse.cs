namespace FootballManager.Api.Responses.Training;

public class TrainingWeekResponse
{
    public string? StartDate { get; set; }
    public List<TrainingDayResponse> Days { get; set; } = [];
}
