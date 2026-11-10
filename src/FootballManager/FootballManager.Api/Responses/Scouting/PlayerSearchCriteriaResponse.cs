using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;

namespace FootballManager.Api.Responses.Scouting;

public class PlayerSearchCriteriaResponse
{
    public List<SearchQuickFilter>? QuickFilters { get; set; }
    public SearchInterestType? InterestType { get; set; }
    public SearchInterestLevel? InterestLevel { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public List<PitchPosition>? Positions { get; set; }
    public PositionFamiliarity? PositionFamiliarity { get; set; }
    public string? MinScoutingGrade { get; set; }
}
