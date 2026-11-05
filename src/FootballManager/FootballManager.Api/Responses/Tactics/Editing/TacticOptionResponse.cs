namespace FootballManager.Api.Responses.Tactics.Editing;

public class TacticOptionResponse
{
    /// <summary>The game's English identifier, as set_tactic takes it.</summary>
    public required string Key { get; set; }

    /// <summary>The game's label, in its display language.</summary>
    public string? Label { get; set; }
}
