namespace FootballManager.Api.Responses;

public class RoleCandidatesResponse
{
    public required string Role { get; set; }
    public string? ShortName { get; set; }
    public List<string> KeyAttributes { get; set; } = [];
    public List<PlayerRoleRatingResponse> Players { get; set; } = [];
}
