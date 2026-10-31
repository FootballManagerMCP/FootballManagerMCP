namespace FootballManager.Api.Responses.Tactics;

public class PlayerRoleRatingResponse
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }

    /// <summary>0-20, the game's stars (20 = five).</summary>
    public required string Rating { get; set; }
}
