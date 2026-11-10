namespace FootballManager.Domain.Scouting;

/// <summary>The player search's quick filters: one-click filters on top of its rules.</summary>
public enum SearchQuickFilter
{
    TransferListed,
    ListedForLoan,

    /// <summary>Without a contract (free agents).</summary>
    OutOfContract,

    /// <summary>At a club, with the contract running out.</summary>
    ContractExpiring,

    HomeGrown,
    Unhappy,

    /// <summary>No weaker foot.</summary>
    TwoFooted,

    Goalkeepers,
    Defenders,
    Midfielders,
    Attackers,

    /// <summary>Players the club's scouting knows well.</summary>
    HighKnowledge,

    NoWorkPermitNeeded
}
