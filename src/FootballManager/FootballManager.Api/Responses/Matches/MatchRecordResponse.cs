namespace FootballManager.Api.Responses.Matches;

public class MatchRecordResponse
{
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public double ExpectedGoalsFor { get; set; }
    public double ExpectedGoalsAgainst { get; set; }
}
