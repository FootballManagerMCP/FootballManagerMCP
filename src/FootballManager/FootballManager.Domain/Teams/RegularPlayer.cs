namespace FootballManager.Domain.Teams;

/// <summary>A player a team starts, with where and how they usually play.</summary>
public class RegularPlayer
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }
    public int Starts { get; set; }

    /// <summary>Their most common position code with the ball (e.g. "AMR"), and role key.</summary>
    public string? Position { get; set; }

    public string? Role { get; set; }
    public string? OutOfPossessionPosition { get; set; }
    public string? OutOfPossessionRole { get; set; }
}
