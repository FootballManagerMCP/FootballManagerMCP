namespace FootballManager.Api.Responses.Competitions;

public class CompetitionPlayerStatsResponse
{
    public required CompetitionResponse Competition { get; set; }

    /// <summary>The stats read, each with the game's name for it.</summary>
    public List<CompetitionStatColumnResponse> Stats { get; set; } = [];

    public required string SortedBy { get; set; }

    /// <summary>How many players of the sort stat's top 100 passed the filters, before the limit.</summary>
    public int Matched { get; set; }

    public List<CompetitionPlayerResponse> Players { get; set; } = [];
}
