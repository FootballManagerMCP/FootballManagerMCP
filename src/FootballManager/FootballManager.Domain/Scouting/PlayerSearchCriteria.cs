using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Scouting;

/// <summary>
/// A player search to run: it replaces the search's rules and quick filters (whatever isn't given is off); the
/// interest setting only changes when given.
/// </summary>
public class PlayerSearchCriteria
{
    public List<SearchQuickFilter> QuickFilters { get; set; } = [];

    public SearchInterestType? InterestType { get; set; }
    public SearchInterestLevel? InterestLevel { get; set; }

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    /// <summary>Any of these positions; none for any position.</summary>
    public List<PitchPosition> Positions { get; set; } = [];

    /// <summary>How well he must play them; natural when positions are given without it.</summary>
    public PositionFamiliarity? PositionFamiliarity { get; set; }

    /// <summary>The scouts' grade he needs at least, as the game writes it: "A+", "B", "C-".</summary>
    public string? MinScoutingGrade { get; set; }
}
