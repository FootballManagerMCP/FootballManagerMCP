using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Scouting;

/// <summary>What the game's player search filters on.</summary>
public class PlayerSearchFiltersResponse
{
    public List<SearchQuickFilter> QuickFilters { get; set; } = [];
    public SearchInterestType? InterestType { get; set; }
    public SearchInterestLevel? InterestLevel { get; set; }

    /// <summary>The standing rules switched on, with the game's labels.</summary>
    public List<SearchRuleResponse> Rules { get; set; } = [];

    /// <summary>Conditions added in the game's filter editor beyond these, not read one by one.</summary>
    public int? OtherConditions { get; set; }
}
