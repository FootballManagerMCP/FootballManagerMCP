using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Matches;

/// <summary>Where a player lined up in one phase of a match, and in what role.</summary>
public class MatchPosition
{
    public TacticalPosition? Position { get; set; }
    public string? Role { get; set; }
    public string? RoleShortName { get; set; }
}
