namespace FootballManager.Api.Responses.Tactics;

public class SlotPositionResponse
{
    public int Index { get; set; }

    /// <summary>The game's label: position and role abbreviations in its display language, e.g. "MOD (AI)".</summary>
    public string? InPossession { get; set; }

    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }

    /// <summary>The player's tactical effectiveness there, 0-100.</summary>
    public string? Effectiveness { get; set; }
}
