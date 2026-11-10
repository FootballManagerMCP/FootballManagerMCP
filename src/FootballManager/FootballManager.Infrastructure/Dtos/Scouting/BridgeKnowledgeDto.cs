namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>How well the club's scouting knows a player: a level out of a maximum, and its name.</summary>
internal sealed class BridgeKnowledgeDto
{
    public int? ActualValue { get; set; }
    public int? MaxValue { get; set; }
    public string? Text { get; set; }
}
