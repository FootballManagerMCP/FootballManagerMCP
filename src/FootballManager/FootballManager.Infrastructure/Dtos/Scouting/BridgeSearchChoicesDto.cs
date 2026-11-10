namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The rules whose values a search can be given, with what they take.</summary>
internal sealed class BridgeSearchChoicesDto
{
    public BridgeSearchRuleDto Age { get; set; } = new();
    public BridgeSearchRuleDto Position { get; set; } = new();
    public BridgeSearchRuleDto Grade { get; set; } = new();
}
