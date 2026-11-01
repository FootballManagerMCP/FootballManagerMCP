namespace FootballManager.Domain.Matches;

/// <summary>A fixture of the user's team, played or upcoming, seen from the user's side.</summary>
public class MatchSummary
{
    public DateOnly Date { get; set; }
    public string? Competition { get; set; }
    public bool IsFriendly { get; set; }
    public bool IsPlayed { get; set; }
    public required MatchSide Home { get; set; }
    public required MatchSide Away { get; set; }

    /// <summary>The user's team, which decides which side is "us".</summary>
    public int TeamId { get; set; }

    /// <summary>The shape the user's team lined up in with the ball, e.g. "4-2-2-2"; null until played.</summary>
    public string? Formation { get; set; }

    public Venue Venue => Home.TeamId == TeamId ? Venue.Home : Venue.Away;
    public MatchSide Us => Venue == Venue.Home ? Home : Away;
    public MatchSide Them => Venue == Venue.Home ? Away : Home;

    public MatchOutcome? Outcome => !IsPlayed || Us.Goals is null || Them.Goals is null ? null
        : Us.Goals > Them.Goals ? MatchOutcome.Win
        : Us.Goals < Them.Goals ? MatchOutcome.Loss
        : MatchOutcome.Draw;
}
