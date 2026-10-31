namespace FootballManager.Infrastructure.Dtos.Tactics;

/// <summary>A score record from the game (a star range or an effectiveness score); -1 means it isn't known.</summary>
internal sealed class BridgeRatingDto
{
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
}
