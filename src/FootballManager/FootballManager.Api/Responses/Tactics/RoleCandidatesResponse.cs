namespace FootballManager.Api.Responses.Tactics;

public class RoleCandidatesResponse
{
    /// <summary>The game's identifier for the role, as set_tactic takes it.</summary>
    public string? Key { get; set; }

    public required string Role { get; set; }
    public string? ShortName { get; set; }
    public List<string> KeyAttributes { get; set; } = [];
    public List<PlayerRoleRatingResponse> Players { get; set; } = [];
}
