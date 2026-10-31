namespace FootballManager.Api.Responses.Tactics.Effectiveness;

/// <summary>Scores are 0-100: "49", or "45-52" while the game's knowledge is incomplete.</summary>
public class TacticalEffectivenessResponse
{
    public string? Overall { get; set; }
    public PlayerAbilityScoresResponse? PlayerAbility { get; set; }
    public PlayerBuyInScoresResponse? PlayerBuyIn { get; set; }
    public TacticalUnderstandingScoresResponse? TacticalUnderstanding { get; set; }
    public NextMatchScoresResponse? NextMatch { get; set; }
    public List<string>? Warnings { get; set; }
}
