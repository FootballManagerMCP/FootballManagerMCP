namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>One line of a report: a short form (a range, a label) and the scout's sentence.</summary>
internal sealed class BridgeReportLineDto
{
    public string? ShortString { get; set; }
    public string? LongString { get; set; }
}
