namespace FootballManager.Api.Responses.Scouting;

public class FocusDraftResponse
{
    public required string Name { get; set; }

    /// <summary>"Nation: Espagne", "Region: Europe centrale (Europe)".</summary>
    public List<string>? Areas { get; set; }

    /// <summary>Position codes; null means any position.</summary>
    public List<string>? Positions { get; set; }

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int? MinAbilityStars { get; set; }
    public int? MinPotentialStars { get; set; }
}
