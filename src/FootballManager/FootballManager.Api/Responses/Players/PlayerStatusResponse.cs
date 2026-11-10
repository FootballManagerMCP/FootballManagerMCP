namespace FootballManager.Api.Responses.Players;

/// <summary>One of the statuses the game shows next to a player.</summary>
public class PlayerStatusResponse
{
    /// <summary>The game's English key, e.g. "FutureTransfer".</summary>
    public string? Key { get; set; }

    public string? Label { get; set; }
    public string? Detail { get; set; }
}
