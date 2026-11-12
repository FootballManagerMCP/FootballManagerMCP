namespace FootballManager.Domain.Competitions;

/// <summary>A competition's players ranked by a stat, each with his numbers and what the user's club knows of him.</summary>
public class CompetitionPlayerStats
{
    public required CompetitionInfo Competition { get; set; }

    /// <summary>The stats read, each with the game's name for it.</summary>
    public List<CompetitionStatColumn> Stats { get; set; } = [];

    public CompetitionStat SortedBy { get; set; }

    /// <summary>How many players of the sort stat's table passed the filters, before the limit.</summary>
    public int Matched { get; set; }

    public List<CompetitionPlayer> Players { get; set; } = [];
}
