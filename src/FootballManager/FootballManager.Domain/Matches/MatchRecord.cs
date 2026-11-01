namespace FootballManager.Domain.Matches;

/// <summary>Results and goals over a set of played matches, with the chances behind them (xG).</summary>
public class MatchRecord
{
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public double ExpectedGoalsFor { get; set; }
    public double ExpectedGoalsAgainst { get; set; }

    public static MatchRecord Of(IEnumerable<MatchSummary> matches)
    {
        var played = matches.Where(m => m.Outcome is not null).ToList();

        return new MatchRecord
        {
            Played = played.Count,
            Won = played.Count(m => m.Outcome == MatchOutcome.Win),
            Drawn = played.Count(m => m.Outcome == MatchOutcome.Draw),
            Lost = played.Count(m => m.Outcome == MatchOutcome.Loss),
            GoalsFor = played.Sum(m => m.Us.Goals ?? 0),
            GoalsAgainst = played.Sum(m => m.Them.Goals ?? 0),
            ExpectedGoalsFor = played.Sum(m => m.Us.ExpectedGoals ?? 0),
            ExpectedGoalsAgainst = played.Sum(m => m.Them.ExpectedGoals ?? 0)
        };
    }
}
