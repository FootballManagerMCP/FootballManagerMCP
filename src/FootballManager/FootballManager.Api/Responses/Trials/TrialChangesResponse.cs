using FootballManager.Api.Responses.Scouting;

namespace FootballManager.Api.Responses.Trials;

public class TrialChangesResponse
{
    public bool Applied { get; set; }

    /// <summary>For trials offered: how long each lasts.</summary>
    public int? Weeks { get; set; }

    /// <summary>The players changed, or who would be on a dry run.</summary>
    public int Count { get; set; }

    /// <summary>The game's menu entry for the first player, in its display language. A player whose entry differs has his own.</summary>
    public string? Choice { get; set; }

    /// <summary>What the game asks the user to confirm, for the first player. A player whose confirmation says something else has his own.</summary>
    public string? Confirmation { get; set; }

    public List<TrialPlayerResponse> Players { get; set; } = [];
    public List<SkippedPlayerResponse>? Skipped { get; set; }

    /// <summary>Ids found in no list the tools reach players through.</summary>
    public List<int>? NotFound { get; set; }

    public List<string>? Notes { get; set; }
}
