namespace FootballManager.Api.Responses.Matches;

public class PenaltyKickResponse
{
    public bool IsOurs { get; set; }
    public string? Taker { get; set; }
    public bool Scored { get; set; }
}
