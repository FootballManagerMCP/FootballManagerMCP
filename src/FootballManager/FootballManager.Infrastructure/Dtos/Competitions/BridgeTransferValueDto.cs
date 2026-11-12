namespace FootballManager.Infrastructure.Dtos.Competitions;

/// <summary>A player's value as the game estimates it for the user, and his club's asking price (-1 when none).</summary>
internal sealed class BridgeTransferValueDto
{
    public long? AskingPrice { get; set; }

    /// <summary>Formatted by the game, e.g. "0 €" or "40K € - 55K €".</summary>
    public string? TransferValueRangeString { get; set; }
}
