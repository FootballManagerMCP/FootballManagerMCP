namespace FootballManager.Api.Responses.Tactics;

public class RoleRatingResponse
{
    public required string Role { get; set; }
    public string? ShortName { get; set; }
    public string? Rating { get; set; }
}
