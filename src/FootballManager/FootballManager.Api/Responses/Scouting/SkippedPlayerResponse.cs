namespace FootballManager.Api.Responses.Scouting;

public class SkippedPlayerResponse
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }
    public required string Reason { get; set; }
}
