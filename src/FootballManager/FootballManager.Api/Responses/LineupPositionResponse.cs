namespace FootballManager.Api.Responses;

public class LineupPositionResponse
{
    public int Index { get; set; }
    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public required PositionAssignmentResponse InPossession { get; set; }
    public required PositionAssignmentResponse OutOfPossession { get; set; }
}
