namespace FootballManager.Domain.Scouting;

/// <summary>A player search run, or planned by a dry run.</summary>
public class PlayerSearchChange
{
    /// <summary>False for a dry run, which only checks.</summary>
    public bool Applied { get; set; }

    /// <summary>The search's filters before.</summary>
    public required PlayerSearchFilters Before { get; set; }

    public required PlayerSearchCriteria Planned { get; set; }

    /// <summary>The search as the game holds it after, its filters read back; null for a dry run.</summary>
    public PlayerSearchStatus? After { get; set; }

    /// <summary>How many players the search found; null for a dry run.</summary>
    public int? Total { get; set; }

    public List<string> Notes { get; set; } = [];
}
