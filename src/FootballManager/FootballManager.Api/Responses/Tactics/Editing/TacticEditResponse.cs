namespace FootballManager.Api.Responses.Tactics.Editing;

public class TacticEditResponse
{
    public int Slot { get; set; }

    /// <summary>False for a dry run: nothing was written.</summary>
    public bool Applied { get; set; }

    public List<TacticEditStepResponse> Steps { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}
