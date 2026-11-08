namespace FootballManager.Domain.Teams;

/// <summary>How a team played over a set of matches, and what its opponents managed against it, per match.</summary>
public class StyleProfile
{
    /// <summary>The matches with numbers that the averages cover.</summary>
    public int Matches { get; set; }

    public required StyleAverages For { get; set; }
    public required StyleAverages Against { get; set; }
}
