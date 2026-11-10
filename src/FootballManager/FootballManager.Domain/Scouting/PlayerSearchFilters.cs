namespace FootballManager.Domain.Scouting;

/// <summary>What the game's player search filters on, as its screen holds it.</summary>
public class PlayerSearchFilters
{
    public List<SearchQuickFilter> QuickFilters { get; set; } = [];

    public SearchInterestType? InterestType { get; set; }

    /// <summary>Null when interest isn't considered.</summary>
    public SearchInterestLevel? InterestLevel { get; set; }

    /// <summary>The standing rules switched on.</summary>
    public List<SearchRule> Rules { get; set; } = [];

    /// <summary>
    /// Conditions the game counts beyond <see cref="Rules"/> and <see cref="QuickFilters"/>: ones added in its filter
    /// editor (nationality, value, a statistic, ...), which aren't read one by one.
    /// </summary>
    public int? OtherConditions { get; set; }
}
