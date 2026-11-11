namespace FootballManager.Domain.Players;

/// <summary>
/// One of the statuses the game shows next to a player (its status icons): a transfer agreed, transfer or loan
/// listed, injured, free agent, watched by other clubs, and so on.
/// </summary>
public class PlayerStatus
{
    /// <summary>The game's English key for it (FM.UI.PlayerStatusFlag), e.g. "FutureTransfer" or "TransferListed".</summary>
    public string? Key { get; set; }

    /// <summary>As the game names it, e.g. "Transfer agreed".</summary>
    public string? Label { get; set; }

    /// <summary>The game's details, e.g. the club a player will join and when, or a listing's asking price.</summary>
    public string? Detail { get; set; }
}
