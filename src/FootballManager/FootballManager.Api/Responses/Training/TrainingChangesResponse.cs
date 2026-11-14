namespace FootballManager.Api.Responses.Training;

public class TrainingChangesResponse
{
    public bool Applied { get; set; }
    public List<PlayerTrainingChangeResponse> Players { get; set; } = [];
    public List<SessionChangeResponse> Sessions { get; set; } = [];
    public List<string>? Notes { get; set; }
}
