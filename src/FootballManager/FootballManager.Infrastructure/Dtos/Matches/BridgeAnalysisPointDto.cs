namespace FootballManager.Infrastructure.Dtos.Matches;

/// <summary>One finding of the game's post-match analysis, in its display language.</summary>
internal sealed class BridgeAnalysisPointDto
{
    /// <summary>The game's grouping of findings (e.g. shots, box entries); it names none of them.</summary>
    public int? AnalysisPointCategory { get; set; }

    public string? AnalysisPointString { get; set; }
}
