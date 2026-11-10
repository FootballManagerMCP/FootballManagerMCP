namespace FootballManager.Api.Responses.Scouting;

public class RecruitmentFocusResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>The game still shows a removed focus under an id it gave a newer one; null otherwise.</summary>
    public bool? ShowsRemovedFocus { get; set; }
    public string? Status { get; set; }
    public bool? IsComplete { get; set; }
    public bool? IsStruggling { get; set; }
    public string? Advice { get; set; }
    public string? Areas { get; set; }
    public string? Scouts { get; set; }
    public string? CreatedBy { get; set; }
    public string? Created { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    /// <summary>On the game's 0-20 star scale.</summary>
    public string? MinAbilityStars { get; set; }
    public string? MinPotentialStars { get; set; }

    /// <summary>Position codes; null means any position.</summary>
    public List<string>? Positions { get; set; }
    public List<string>? OutOfPossessionPositions { get; set; }

    public int? RecommendedPlayers { get; set; }
    public int? CurrentPlayers { get; set; }
    public int? NearMissPlayers { get; set; }
}
