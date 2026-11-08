namespace FootballManager.Api.Responses.Teams;

public class RegularPlayerResponse
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }
    public int Starts { get; set; }

    /// <summary>Most common position with the ball (FM's English code) and role key.</summary>
    public string? Position { get; set; }

    public string? Role { get; set; }
    public string? OutOfPossessionPosition { get; set; }
    public string? OutOfPossessionRole { get; set; }
}
