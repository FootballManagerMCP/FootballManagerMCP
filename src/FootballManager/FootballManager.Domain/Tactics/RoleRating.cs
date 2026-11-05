namespace FootballManager.Domain.Tactics;

/// <summary>A role a player could fill at a position, and how well the game rates them in it.</summary>
public class RoleRating
{
    /// <summary>The role's key, the game's English identifier (e.g. "CentreForward"), as tactic edits take it.</summary>
    public string? Key { get; set; }

    public required string Role { get; set; }
    public string? ShortName { get; set; }
    public Rating? Rating { get; set; }
}
