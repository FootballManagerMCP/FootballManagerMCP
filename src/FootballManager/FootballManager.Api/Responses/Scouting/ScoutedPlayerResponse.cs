using FootballManager.Api.Responses.Players;

namespace FootballManager.Api.Responses.Scouting;

public class ScoutedPlayerResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }
    public string? Club { get; set; }
    public string? Nation { get; set; }
    public string? Positions { get; set; }

    /// <summary>A position code, e.g. "DC".</summary>
    public string? BestPosition { get; set; }

    /// <summary>The game's English role keys.</summary>
    public string? BestRole { get; set; }
    public string? BestOutOfPossessionRole { get; set; }

    /// <summary>0-12, with the game's label.</summary>
    public int? Grade { get; set; }
    public string? GradeLabel { get; set; }
    public string? Recommendation { get; set; }

    /// <summary>The scouts' stars on the game's 0-20 scale: "16", or "14-16" while knowledge is partial.</summary>
    public string? AbilityStars { get; set; }
    public string? PotentialStars { get; set; }

    /// <summary>"5/5", with the game's name for it.</summary>
    public string? KnowledgeLevel { get; set; }
    public string? Knowledge { get; set; }

    public string? Value { get; set; }
    public string? Wage { get; set; }
    public string? StrongestArea { get; set; }
    public string? WeakestArea { get; set; }

    /// <summary>Only sent when true.</summary>
    public bool? IsShortlisted { get; set; }
    public bool? IsBeingScouted { get; set; }
    public int? DaysSinceLastScouted { get; set; }

    /// <summary>The statuses the game shows for him; null when there are none.</summary>
    public List<PlayerStatusResponse>? Statuses { get; set; }

    /// <summary>Category ("technical", "mental", ...) to attribute name to value: "12", "11-14" or null if masked.</summary>
    public Dictionary<string, Dictionary<string, string?>>? Attributes { get; set; }

    public ScoutReportResponse? Report { get; set; }

    public int? CurrentAbility { get; set; }
    public int? PotentialAbility { get; set; }
}
