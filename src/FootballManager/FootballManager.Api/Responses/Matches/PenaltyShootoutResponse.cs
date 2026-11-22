namespace FootballManager.Api.Responses.Matches;

public class PenaltyShootoutResponse
{
    /// <summary>The user's team's penalties scored.</summary>
    public int Scored { get; set; }

    public int Conceded { get; set; }
    public List<PenaltyKickResponse> Kicks { get; set; } = [];
}
