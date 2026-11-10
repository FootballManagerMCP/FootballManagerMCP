namespace FootballManager.Domain.Scouting;

/// <summary>Which move a player's interest is judged for in the player search.</summary>
public enum SearchInterestType
{
    TransferOrLoan,
    TransferOnly,
    LoanOnly,

    /// <summary>Interest isn't taken into account.</summary>
    NotConsidered
}
