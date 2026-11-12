namespace FootballManager.Domain.Training;

/// <summary>A session of the schedule to replace.</summary>
public class SessionEdit
{
    /// <summary>yyyy-MM-dd, in this week or the next.</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>The session's place in the day, 0 to 2.</summary>
    public int Slot { get; set; }

    /// <summary>The new session's id (TrainingOverview.SessionTypes).</summary>
    public int Session { get; set; }
}
