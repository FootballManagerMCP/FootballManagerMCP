namespace FootballManager.Api.Responses;

public class PlayerResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }
    public string? Positions { get; set; }
    public string? BestRole { get; set; }
    public string? Personality { get; set; }
    public string? Morale { get; set; }
    public int? Condition { get; set; }
    public int? MatchSharpness { get; set; }
    public bool IsInjured { get; set; }
    public double? AverageRating { get; set; }

    /// <summary>Category ("technical", "mental", ...) to attribute name to value: "12", "11-14" or null if masked.</summary>
    public Dictionary<string, Dictionary<string, string?>>? Attributes { get; set; }

    public int? CurrentAbility { get; set; }
    public int? PotentialAbility { get; set; }
}
