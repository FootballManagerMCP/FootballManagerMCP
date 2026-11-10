namespace FootballManager.Api.Responses.Matches;

public class MatchSubstitutionResponse
{
    public int? Minute { get; set; }
    public int? OffPlayerId { get; set; }
    public string? Off { get; set; }
    public int? OnPlayerId { get; set; }
    public string? On { get; set; }

    /// <summary>The slot's position with the ball, as FM writes it.</summary>
    public string? Position { get; set; }

    /// <summary>Only sent when the player who came off was injured.</summary>
    public bool? Injury { get; set; }
}
