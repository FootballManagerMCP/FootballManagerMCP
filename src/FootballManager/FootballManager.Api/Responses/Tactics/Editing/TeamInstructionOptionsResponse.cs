namespace FootballManager.Api.Responses.Tactics.Editing;

public class TeamInstructionOptionsResponse
{
    public required string Key { get; set; }
    public string? Label { get; set; }

    /// <summary>Where the game files it, e.g. "InPossessionDefault", "BuildUp", "HighPress", "OutOfPossessionDefault".</summary>
    public string? Phase { get; set; }

    /// <summary>The option the active tactic holds.</summary>
    public string? Current { get; set; }

    public List<TacticOptionResponse> Options { get; set; } = [];
}
