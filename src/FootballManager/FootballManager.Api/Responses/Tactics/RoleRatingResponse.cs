namespace FootballManager.Api.Responses.Tactics;

public class RoleRatingResponse
{
    /// <summary>The game's identifier for the role, as set_tactic takes it.</summary>
    public string? Key { get; set; }

    public required string Role { get; set; }
    public string? ShortName { get; set; }
    public string? Rating { get; set; }
}
