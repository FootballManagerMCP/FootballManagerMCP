using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Scouting;

public class ScoutingOverviewResponse
{
    public int? RecommendedPlayers { get; set; }
    public int? HighlyRecommendedPlayers { get; set; }
    public int? RequestedReports { get; set; }
    public int? ShortlistedPlayers { get; set; }
    public int? ScoutedPlayers { get; set; }
    public ScoutingBudgetResponse? Budget { get; set; }
    public List<ScoutResponse> Scouts { get; set; } = [];
    public FocusListFilter? FocusesShown { get; set; }
    public List<RecruitmentFocusResponse> Focuses { get; set; } = [];
}
