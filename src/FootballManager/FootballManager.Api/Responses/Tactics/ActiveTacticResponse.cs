namespace FootballManager.Api.Responses.Tactics;

public class ActiveTacticResponse
{
    public string? InPossessionFormation { get; set; }
    public string? OutOfPossessionFormation { get; set; }

    /// <summary>The game's identifier, as set_tactic takes it; absent for a custom shape.</summary>
    public string? InPossessionFormationKey { get; set; }

    public string? OutOfPossessionFormationKey { get; set; }
    public string? Mentality { get; set; }
    public string? MentalityKey { get; set; }
    public string? Style { get; set; }
    public string? StyleKey { get; set; }

    /// <summary>Team instruction key to the chosen option's key (e.g. "Tempo": "MuchHigherTempo").</summary>
    public Dictionary<string, string?> TeamInstructions { get; set; } = [];

    public List<LineupPositionResponse> Lineup { get; set; } = [];
}
