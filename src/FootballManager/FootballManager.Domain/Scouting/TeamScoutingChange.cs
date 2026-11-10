namespace FootballManager.Domain.Scouting;

/// <summary>Scouts sent to watch a team's players, one assignment each, or planned by a dry run.</summary>
public class TeamScoutingChange
{
    /// <summary>False for a dry run, which only checks.</summary>
    public bool Applied { get; set; }

    public int TeamId { get; set; }
    public required string TeamName { get; set; }

    /// <summary>The scouting budget and spending before the scouts are sent.</summary>
    public ScoutingBudget? Budget { get; set; }

    /// <summary>The players sent a scout (or who would be), each with the menu entry picked for him.</summary>
    public List<PlayerScoutingChange> Players { get; set; } = [];

    /// <summary>Players left out: already watched, or the game doesn't offer what was asked for them.</summary>
    public List<SkippedPlayer> Skipped { get; set; } = [];

    /// <summary>What stopped the run before the end, when something did.</summary>
    public List<string> Notes { get; set; } = [];
}
