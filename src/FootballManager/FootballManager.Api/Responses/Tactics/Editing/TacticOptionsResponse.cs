namespace FootballManager.Api.Responses.Tactics.Editing;

public class TacticOptionsResponse
{
    /// <summary>The slot the team plays now, 1 to 3.</summary>
    public int ActiveSlot { get; set; }

    public string? Style { get; set; }
    public List<TacticOptionResponse> Styles { get; set; } = [];
    public string? Mentality { get; set; }
    public List<TacticOptionResponse> Mentalities { get; set; } = [];

    /// <summary>The formation of each phase now; absent for a custom shape.</summary>
    public required ByPhaseResponse<string> Formation { get; set; }

    public required ByPhaseResponse<List<TacticOptionResponse>> Formations { get; set; }
    public List<TeamInstructionOptionsResponse> TeamInstructions { get; set; } = [];
    public List<PlayerInstructionGroupResponse> PlayerInstructions { get; set; } = [];

    /// <summary>Every role key of each phase; which ones a position offers, with ratings, comes from get_role_fit.</summary>
    public required ByPhaseResponse<List<string>> Roles { get; set; }
}
