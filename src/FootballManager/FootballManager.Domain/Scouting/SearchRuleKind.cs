namespace FootballManager.Domain.Scouting;

/// <summary>The player search's standing rules, which its filter editor lists first.</summary>
public enum SearchRuleKind
{
    Age,
    Position,
    ScoutingGrade,
    Contract,
    TransferStatus,
    LoanStatus,
    InPossessionRole,
    OutOfPossessionRole
}
