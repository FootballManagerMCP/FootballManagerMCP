namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>One strength or weakness in a report.</summary>
internal sealed class BridgeProConDto
{
    /// <summary>A heading, e.g. "Versatility".</summary>
    public string? ProConsShortString { get; set; }

    /// <summary>The scout's sentence, e.g. "Can only play in one position."</summary>
    public string? ProConsLongString { get; set; }
}
