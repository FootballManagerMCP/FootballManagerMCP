namespace FootballManager.Domain.Competitions;

/// <summary>One stat's table on a competition's stats screen: the players the game ranks best in it, best first.</summary>
public class CompetitionStatTable
{
    public CompetitionStat Stat { get; set; }

    /// <summary>The stat as the game names it, in its display language.</summary>
    public string? Label { get; set; }

    public List<CompetitionStatRow> Rows { get; set; } = [];
}
