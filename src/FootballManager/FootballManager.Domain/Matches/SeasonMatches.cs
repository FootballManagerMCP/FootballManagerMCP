namespace FootballManager.Domain.Matches;

/// <summary>The user's season so far: the matches played, the next ones, and the record over competitive matches.</summary>
public class SeasonMatches
{
    public required string TeamName { get; set; }

    /// <summary>Competitive matches only; friendlies never count.</summary>
    public required MatchRecord Overall { get; set; }
    public required MatchRecord Home { get; set; }
    public required MatchRecord Away { get; set; }

    public List<MatchSummary> Played { get; set; } = [];
    public List<MatchSummary> Upcoming { get; set; } = [];
}
