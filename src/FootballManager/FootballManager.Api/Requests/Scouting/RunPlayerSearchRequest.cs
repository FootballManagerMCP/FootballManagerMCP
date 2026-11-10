using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;

namespace FootballManager.Api.Requests.Scouting;

/// <summary>A player search: it replaces the search's rules and quick filters; the interest only changes when given.</summary>
public class RunPlayerSearchRequest
{
    public List<SearchQuickFilter>? QuickFilters { get; set; }

    public SearchInterestType? InterestType { get; set; }
    public SearchInterestLevel? InterestLevel { get; set; }

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    /// <summary>Any of these positions; none for any position.</summary>
    public List<PitchPosition>? Positions { get; set; }

    /// <summary>Natural when positions are given without it.</summary>
    public PositionFamiliarity? PositionFamiliarity { get; set; }

    /// <summary>"A+" to "F".</summary>
    public string? MinScoutingGrade { get; set; }
}
