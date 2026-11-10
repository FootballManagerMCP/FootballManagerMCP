using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Matches;

/// <summary>Where a player lined up in one phase of a match, and in what role.</summary>
public class MatchPosition
{
    public TacticalPosition? Position { get; set; }

    /// <summary>The role's key, the game's English identifier (e.g. "CentreForward"), known whoever played it.</summary>
    public string? RoleKey { get; set; }

    /// <summary>The role's name in the game's language, when the player's role table there names it.</summary>
    public string? Role { get; set; }
    public string? RoleShortName { get; set; }
}
