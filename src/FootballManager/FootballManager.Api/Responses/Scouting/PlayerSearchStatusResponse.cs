namespace FootballManager.Api.Responses.Scouting;

/// <summary>The game's player search as its screen shows it, in the game's display language.</summary>
public class PlayerSearchStatusResponse
{
    public string? Status { get; set; }
    public string? Interest { get; set; }
    public string? Attributes { get; set; }
}
