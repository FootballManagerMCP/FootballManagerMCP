namespace FootballManager.Domain.Training;

/// <summary>One of a training day's three sessions.</summary>
public class TrainingSession
{
    /// <summary>The session's place in the day, 0 to 2 in the day's order.</summary>
    public int Slot { get; set; }

    /// <summary>The session's id (its category times 100, plus its place in the category), e.g. 303.</summary>
    public int Type { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>The category's id (see TrainingOverview.SessionTypes), or -1 when no training can take place.</summary>
    public int Category { get; set; }

    /// <summary>Whether the user can change it (not on a past day, nor one the team can't train, travels or plays).</summary>
    public bool CanEdit { get; set; }
}
