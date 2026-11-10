using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Scouting;

public class ScoutedPlayerListResponse
{
    public ScoutingSource Source { get; set; }
    public RecruitmentFocusResponse? Focus { get; set; }
    public FocusPool? Pool { get; set; }

    /// <summary>The search screen's state, for the player search.</summary>
    public PlayerSearchStatusResponse? Search { get; set; }

    /// <summary>The players listed here.</summary>
    public int Count { get; set; }

    /// <summary>How many the list holds; more than <see cref="Count"/> when a limit cut it short.</summary>
    public int Total { get; set; }

    public bool? HiddenDataMasked { get; set; }
    public List<ScoutedPlayerResponse> Players { get; set; } = [];
}
