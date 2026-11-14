namespace FootballManager.Domain.Competitions;

/// <summary>A stat read from the stats screen, with the game's name for it.</summary>
public class CompetitionStatColumn
{
    public CompetitionStat Stat { get; set; }

    /// <summary>In the game's display language, e.g. "Tacles remportés".</summary>
    public string? Label { get; set; }
}
