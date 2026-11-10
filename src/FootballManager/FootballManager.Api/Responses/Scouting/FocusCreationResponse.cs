namespace FootballManager.Api.Responses.Scouting;

public class FocusCreationResponse
{
    public bool Applied { get; set; }
    public required FocusDraftResponse Planned { get; set; }

    /// <summary>The focus as the game holds it once created.</summary>
    public RecruitmentFocusResponse? Focus { get; set; }

    public List<string>? Notes { get; set; }
}
