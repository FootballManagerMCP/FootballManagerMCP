namespace FootballManager.Domain.Competitions;

/// <summary>A player's value in a stat, and his place in its table (1 for the best; tied players share one).</summary>
public class CompetitionStatValue
{
    public CompetitionStat Stat { get; set; }
    public double Value { get; set; }
    public int Rank { get; set; }
}
