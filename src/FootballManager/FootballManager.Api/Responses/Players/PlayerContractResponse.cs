namespace FootballManager.Api.Responses.Players;

public class PlayerContractResponse
{
    /// <summary>The kind of contract (amateur, part-time, full-time), in the game's language.</summary>
    public string? Status { get; set; }

    /// <summary>Its term: monthly (rolling) or a fixed-term contract.</summary>
    public string? Type { get; set; }

    /// <summary>yyyy-MM-dd; left out when the contract has no end date.</summary>
    public string? EndDate { get; set; }

    /// <summary>As the game writes it, with its period; left out when it doesn't apply.</summary>
    public string? Wage { get; set; }

    /// <summary>The game's estimate of his value.</summary>
    public string? Value { get; set; }

    /// <summary>His club's asking price, when it has set one.</summary>
    public long? AskingPrice { get; set; }

    public string? TransferStatus { get; set; }
}
