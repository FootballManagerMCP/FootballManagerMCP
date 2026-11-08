namespace FootballManager.Domain.Teams;

/// <summary>How a team lines up: the shapes it used and how often, and its most-used starters.</summary>
public class UsualLineup
{
    /// <summary>Shape with the ball (e.g. "4-2-3-1") to the matches it was used in, most used first.</summary>
    public Dictionary<string, int> InPossessionShapes { get; set; } = [];

    public Dictionary<string, int> OutOfPossessionShapes { get; set; } = [];

    /// <summary>The players who started, most starts first.</summary>
    public List<RegularPlayer> Players { get; set; } = [];
}
