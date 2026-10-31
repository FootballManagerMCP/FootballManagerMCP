namespace FootballManager.Api.Responses;

public class ActiveTacticResponse
{
    public string? InPossessionFormation { get; set; }
    public string? OutOfPossessionFormation { get; set; }
    public string? Mentality { get; set; }
    public string? Style { get; set; }

    /// <summary>Team instruction to the chosen option, by the game's own names (e.g. "Tempo": "MuchHigherTempo").</summary>
    public Dictionary<string, string?> TeamInstructions { get; set; } = [];

    public List<LineupPositionResponse> Lineup { get; set; } = [];
}
