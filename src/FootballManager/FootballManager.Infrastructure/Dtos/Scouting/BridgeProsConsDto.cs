namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>A report's strengths and weaknesses.</summary>
internal sealed class BridgeProsConsDto
{
    public List<BridgeProConDto> ProsList { get; set; } = [];
    public List<BridgeProConDto> ConsList { get; set; } = [];
}
