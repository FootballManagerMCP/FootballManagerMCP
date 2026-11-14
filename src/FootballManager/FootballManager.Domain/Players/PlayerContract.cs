namespace FootballManager.Domain.Players;

/// <summary>
/// A player's contract with his club and what he'd cost, as the game shows the user. Text values are in the game's
/// display language.
/// </summary>
public class PlayerContract
{
    /// <summary>The kind of contract, e.g. "Amateur contract", "Part-time contract".</summary>
    public string? Status { get; set; }

    /// <summary>Its term, e.g. "Monthly" (rolling, as amateurs have) or a fixed-term contract.</summary>
    public string? Type { get; set; }

    /// <summary>yyyy-MM-dd; null when the contract has no end date (amateur and non-contract terms).</summary>
    public string? EndDate { get; set; }

    /// <summary>His wage as the game writes it, with its period (e.g. "280 € p/m"); null when it doesn't apply.</summary>
    public string? Wage { get; set; }

    /// <summary>The game's estimate of his value, e.g. "0 €" or "40K € - 55K €".</summary>
    public string? Value { get; set; }

    /// <summary>His club's asking price, when it has set one.</summary>
    public long? AskingPrice { get; set; }

    /// <summary>E.g. "Not set", "Listed for transfer".</summary>
    public string? TransferStatus { get; set; }
}
