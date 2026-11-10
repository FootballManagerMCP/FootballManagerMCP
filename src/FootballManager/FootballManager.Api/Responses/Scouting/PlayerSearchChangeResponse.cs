namespace FootballManager.Api.Responses.Scouting;

public class PlayerSearchChangeResponse
{
    public bool Applied { get; set; }

    /// <summary>What the search filtered on before.</summary>
    public required PlayerSearchFiltersResponse Before { get; set; }

    public required PlayerSearchCriteriaResponse Planned { get; set; }

    /// <summary>The search as the game holds it after; null for a dry run.</summary>
    public PlayerSearchStatusResponse? After { get; set; }

    /// <summary>How many players it found; null for a dry run.</summary>
    public int? Total { get; set; }

    public List<string>? Notes { get; set; }
}
