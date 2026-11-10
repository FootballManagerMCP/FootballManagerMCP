namespace FootballManager.Domain.Scouting;

/// <summary>How interested a player must at least be in the move, as the game rates it.</summary>
public enum SearchInterestLevel
{
    Doubtful,

    /// <summary>Can be persuaded.</summary>
    SlightInterest,

    Interested
}
