namespace FootballManager.Api.Responses.Scouting;

public class TeamScoutingChangeResponse
{
    public bool Applied { get; set; }
    public int TeamId { get; set; }
    public required string TeamName { get; set; }

    /// <summary>The players sent a scout, or who would be on a dry run.</summary>
    public int Count { get; set; }

    /// <summary>The game's menu entry for the first player, in its display language: the same scout and duration for all.</summary>
    public string? Choice { get; set; }

    /// <summary>
    /// What the game asks the user to confirm, for the first player: the costs to expect. A player whose confirmation
    /// says something else has his own.
    /// </summary>
    public string? Confirmation { get; set; }

    public ScoutingBudgetResponse? Budget { get; set; }

    public List<TeamScoutedPlayerResponse> Players { get; set; } = [];
    public List<SkippedPlayerResponse> Skipped { get; set; } = [];
    public List<string>? Notes { get; set; }
}
