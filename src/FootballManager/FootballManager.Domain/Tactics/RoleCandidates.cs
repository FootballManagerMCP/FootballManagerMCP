namespace FootballManager.Domain.Tactics;

/// <summary>A role at a position, what the game looks for in it, and the squad players rated in it.</summary>
public class RoleCandidates
{
    /// <summary>The role's key, the game's English identifier (e.g. "CentreForward"), as tactic edits take it.</summary>
    public string? Key { get; set; }

    public required string Role { get; set; }
    public string? ShortName { get; set; }

    /// <summary>The attributes the role relies on most, in the game's display language.</summary>
    public List<string> KeyAttributes { get; set; } = [];

    public List<PlayerRoleRating> Players { get; set; } = [];
}
