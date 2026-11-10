namespace FootballManager.Domain.Scouting;

/// <summary>A scout's latest report on a player, in the game's display language.</summary>
public class ScoutReport
{
    /// <summary>As the game writes dates, e.g. "21/10/2026".</summary>
    public string? Date { get; set; }

    /// <summary>Who wrote it.</summary>
    public string? Scout { get; set; }

    /// <summary>The scout's view of his current level, e.g. "Star player in the Premiership".</summary>
    public string? Ability { get; set; }

    /// <summary>The scout's view of how far he can go.</summary>
    public string? Potential { get; set; }

    public List<string> Pros { get; set; } = [];
    public List<string> Cons { get; set; } = [];

    /// <summary>The fee he could likely be signed for, as a range.</summary>
    public string? TransferFee { get; set; }

    /// <summary>The wage he would likely ask for, as a range.</summary>
    public string? WageDemands { get; set; }

    /// <summary>How keen he would be to join, e.g. "Slight interest".</summary>
    public string? TransferInterest { get; set; }

    /// <summary>How keen he would be to join on loan.</summary>
    public string? LoanInterest { get; set; }
}
