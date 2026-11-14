namespace FootballManager.Api.Responses.Training;

public class TrainingResponse
{
    public string? Schedule { get; set; }
    public List<TrainingWeekResponse> Weeks { get; set; } = [];
    public List<TrainingSessionCategoryResponse> SessionTypes { get; set; } = [];
    public List<TrainingOptionResponse> FocusOptions { get; set; } = [];
    public List<TrainingOptionResponse> IntensityOptions { get; set; } = [];
    public List<PlayerTrainingResponse> Players { get; set; } = [];
}
