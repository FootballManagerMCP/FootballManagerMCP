namespace FootballManager.Api.Responses.Competitions;

public class CompetitionStatValueResponse
{
    public double Value { get; set; }

    /// <summary>1 for the best; tied players share one.</summary>
    public int Rank { get; set; }
}
