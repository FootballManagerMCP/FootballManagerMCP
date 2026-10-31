namespace FootballManager.Api.Responses;

public class TacticalUnderstandingScoresResponse
{
    public string? Overall { get; set; }
    public string? Comprehension { get; set; }
    public EffectivenessAdviceResponse? ComprehensionAdvice { get; set; }
    public TacticalAreaResponse? InPossessionShape { get; set; }
    public TacticalAreaResponse? OutOfPossessionShape { get; set; }
    public TacticalAreaResponse? BuildUpStrategy { get; set; }
    public TacticalAreaResponse? ChanceCreation { get; set; }
    public TacticalAreaResponse? PassingStyle { get; set; }
    public TacticalAreaResponse? PressingIntensity { get; set; }
    public TacticalAreaResponse? PressingTrap { get; set; }
    public TacticalAreaResponse? LineOfEngagement { get; set; }
    public EffectivenessAdviceResponse? AreasAdvice { get; set; }
    public TacticalComplexityResponse? Complexity { get; set; }
}
