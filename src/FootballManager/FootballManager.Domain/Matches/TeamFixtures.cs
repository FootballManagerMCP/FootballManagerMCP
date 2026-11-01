namespace FootballManager.Domain.Matches;

/// <summary>Every fixture of the user's team this season, played and upcoming, in date order.</summary>
public class TeamFixtures
{
    public required string TeamName { get; set; }
    public List<MatchSummary> Fixtures { get; set; } = [];
}
