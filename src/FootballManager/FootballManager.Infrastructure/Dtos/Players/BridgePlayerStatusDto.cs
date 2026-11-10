namespace FootballManager.Infrastructure.Dtos.Players;

/// <summary>One of a player's status icons, as the game sends it (the short title is left out).</summary>
internal sealed class BridgePlayerStatusDto
{
    /// <summary>An FM.UI.PlayerStatusFlag value.</summary>
    public int? StatusFlag { get; set; }

    public string? StatusTitleFull { get; set; }

    /// <summary>The details, over several lines when long (e.g. every club watching him).</summary>
    public string? StatusReason { get; set; }
}
