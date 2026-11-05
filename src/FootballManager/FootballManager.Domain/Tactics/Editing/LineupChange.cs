namespace FootballManager.Domain.Tactics.Editing;

/// <summary>A player of the starting eleven and where and how they play; anything left out stays as it is.</summary>
public class LineupChange
{
    /// <summary>The player's FM id, as the squad lists it.</summary>
    public int PlayerId { get; set; }

    /// <summary>FM's position codes, e.g. "DCL", "MR", "ST".</summary>
    public ByPhase<string>? Position { get; set; }

    /// <summary>Role keys, e.g. "CentreForward" in possession, "TrackingCentreForward" out of it.</summary>
    public ByPhase<string>? Role { get; set; }

    /// <summary>Player instruction keys for a phase; giving a phase's list replaces all of that phase's instructions.</summary>
    public ByPhase<List<string>>? Instructions { get; set; }
}
