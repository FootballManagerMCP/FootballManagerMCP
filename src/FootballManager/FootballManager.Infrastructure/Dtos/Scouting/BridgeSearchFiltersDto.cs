using FootballManager.Domain.Scouting;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>Everything the search filters on, read in parts (each quick filter and each rule on its own).</summary>
internal sealed class BridgeSearchFiltersDto
{
    public BridgeSearchStateDto State { get; set; } = new();
    public List<SearchQuickFilter> QuickFilters { get; set; } = [];
    public List<BridgeSearchRuleDto> Rules { get; set; } = [];
}
