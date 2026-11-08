using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Teams;

/// <summary>A player of a team in one match: where they played in each phase, and in what role.</summary>
public class TeamLineupEntry
{
    public int? PlayerId { get; set; }
    public string? Name { get; set; }
    public bool IsStarter { get; set; }
    public TacticalPosition? InPossessionPosition { get; set; }

    /// <summary>The in-possession role's key, the game's English identifier (e.g. "WideForward").</summary>
    public string? InPossessionRole { get; set; }

    public TacticalPosition? OutOfPossessionPosition { get; set; }
    public string? OutOfPossessionRole { get; set; }
}
