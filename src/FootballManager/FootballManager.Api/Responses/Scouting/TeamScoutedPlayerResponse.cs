namespace FootballManager.Api.Responses.Scouting;

public class TeamScoutedPlayerResponse
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }

    /// <summary>As the game reads back after the scout was sent; for a dry run, as it is now.</summary>
    public bool? IsBeingScouted { get; set; }

    /// <summary>Only when it says something other than the first player's (outside the scouting network, say).</summary>
    public string? Confirmation { get; set; }
}
