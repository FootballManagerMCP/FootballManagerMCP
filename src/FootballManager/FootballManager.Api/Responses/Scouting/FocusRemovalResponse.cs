namespace FootballManager.Api.Responses.Scouting;

public class FocusRemovalResponse
{
    public bool Applied { get; set; }
    public required RecruitmentFocusResponse Focus { get; set; }
}
