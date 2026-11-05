namespace FootballManager.Api.Responses.Tactics.Editing;

public class PlayerInstructionGroupResponse
{
    public required string Key { get; set; }
    public string? Label { get; set; }
    public bool InPossession { get; set; }

    /// <summary>A player takes at most one of a group's options; none is the default.</summary>
    public List<TacticOptionResponse> Options { get; set; } = [];
}
