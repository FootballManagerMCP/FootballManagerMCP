namespace FootballManager.Api.Responses.Teams;

public class UsualLineupResponse
{
    /// <summary>Shape with the ball to the number of matches it was used in, most used first.</summary>
    public Dictionary<string, int> InPossessionShapes { get; set; } = [];

    public Dictionary<string, int> OutOfPossessionShapes { get; set; } = [];
    public List<RegularPlayerResponse> Players { get; set; } = [];
}
