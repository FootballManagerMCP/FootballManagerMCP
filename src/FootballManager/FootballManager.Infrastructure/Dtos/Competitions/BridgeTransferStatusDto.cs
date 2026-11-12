namespace FootballManager.Infrastructure.Dtos.Competitions;

/// <summary>A player's transfer status as his club has set it, e.g. "Not set" or "Listed for transfer".</summary>
internal sealed class BridgeTransferStatusDto
{
    public BridgeTransferStatusTextDto? TransferStatusDescription { get; set; }
}
