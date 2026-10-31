namespace FootballManager.Api.Responses.Tactics;

public class RoleFitResponse
{
    public required string Position { get; set; }
    public bool InPossession { get; set; }
    public List<RoleCandidatesResponse> Roles { get; set; } = [];
}
