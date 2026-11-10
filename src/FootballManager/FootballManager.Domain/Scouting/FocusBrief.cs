namespace FootballManager.Domain.Scouting;

/// <summary>A new recruitment focus as asked for, before it's checked against what the game offers.</summary>
public class FocusBrief
{
    public required string Name { get; set; }

    /// <summary>Areas by name in the game's language, optionally with their kind: "Espagne", "nation:Espagne".</summary>
    public List<string> Areas { get; set; } = [];

    /// <summary>Position codes ("DL", "MC"); none means any position.</summary>
    public List<string> Positions { get; set; } = [];

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    /// <summary>On the game's 0-20 star scale; the game's defaults when left out.</summary>
    public int? MinAbilityStars { get; set; }
    public int? MinPotentialStars { get; set; }
}
