namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>Something that happened in a match; type 0 is a goal, with the scorer as Player and the assister as Player2.</summary>
internal sealed class BridgeIncidentDto
{
    public int? IncidentType { get; set; }

    /// <summary>The side it happened to: 0 the home team, 1 the away team.</summary>
    public int? TeamSelectionIndex { get; set; }

    /// <summary>The minute as the game writes it, e.g. "21'".</summary>
    public string? TimeString { get; set; }
    public BridgeReferenceDto? Player { get; set; }
    public BridgeReferenceDto? Player2 { get; set; }
}
