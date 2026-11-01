namespace FootballManager.Api.Responses.Matches;

public class MatchLineupPlayerResponse
{
    /// <summary>0-10 for the starting eleven, higher for substitutes.</summary>
    public int SelectionIndex { get; set; }
    public int? PlayerId { get; set; }
    public string? Name { get; set; }
    public bool CameOnAsSubstitute { get; set; }
    public double? Rating { get; set; }
    public required MatchPositionResponse InPossession { get; set; }
    public required MatchPositionResponse OutOfPossession { get; set; }
    public PlayerMatchStatsResponse? Stats { get; set; }
}
