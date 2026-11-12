namespace FootballManager.Api.Responses.Training;

public class TrainingSessionCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TrainingOptionResponse> Sessions { get; set; } = [];
}
