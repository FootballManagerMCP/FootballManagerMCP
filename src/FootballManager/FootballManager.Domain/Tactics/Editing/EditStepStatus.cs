namespace FootballManager.Domain.Tactics.Editing;

public enum EditStepStatus
{
    /// <summary>A dry run's step: nothing was written.</summary>
    Planned,
    Done,
    Failed,

    /// <summary>Not tried, because an earlier step failed.</summary>
    Skipped
}
