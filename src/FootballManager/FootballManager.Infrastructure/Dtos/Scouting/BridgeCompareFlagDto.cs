namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>How a search rule compares, e.g. {"Text": "Entre", "ID": 16}.</summary>
internal sealed class BridgeCompareFlagDto
{
    public string? Text { get; set; }
    public long? Id { get; set; }
}
