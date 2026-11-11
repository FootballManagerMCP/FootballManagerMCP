namespace FootballManager.Api.Requests.Scouting;

public class CreateFocusRequest
{
    public required string Name { get; set; }

    /// <summary>Areas by name in the game's language, optionally with their kind: "Spain", "nation:Spain".</summary>
    public List<string>? Areas { get; set; }

    /// <summary>Position codes ("DL", "MC"); none means any position.</summary>
    public List<string>? Positions { get; set; }

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    /// <summary>On the game's 0-20 star scale (20 is five stars).</summary>
    public int? MinAbilityStars { get; set; }
    public int? MinPotentialStars { get; set; }
}
