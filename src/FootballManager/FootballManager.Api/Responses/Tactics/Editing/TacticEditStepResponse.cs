namespace FootballManager.Api.Responses.Tactics.Editing;

public class TacticEditStepResponse
{
    public required string Action { get; set; }
    public string? Subject { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }

    /// <summary>Planned (dry run), Done, Failed, or Skipped after an earlier failure.</summary>
    public required string Status { get; set; }

    public string? Detail { get; set; }
}
