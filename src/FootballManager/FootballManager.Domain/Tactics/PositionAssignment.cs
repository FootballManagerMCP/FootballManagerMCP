namespace FootballManager.Domain.Tactics;

/// <summary>Where a lineup player stands in one phase (in or out of possession), and in what role.</summary>
public class PositionAssignment
{
    public TacticalPosition? Position { get; set; }

    /// <summary>The role's key, the game's English identifier (e.g. "CentreForward"), as tactic edits take it.</summary>
    public string? RoleKey { get; set; }

    public string? Role { get; set; }
    public string? RoleShortName { get; set; }

    /// <summary>The player's rating in that role, 0-20.</summary>
    public Rating? Rating { get; set; }

    /// <summary>Every role available at the position with the player's rating in it, best first.</summary>
    public List<RoleRating> RoleOptions { get; set; } = [];
}
