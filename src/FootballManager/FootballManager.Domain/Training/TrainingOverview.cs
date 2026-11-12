namespace FootballManager.Domain.Training;

/// <summary>The team's training, as the game's training screens show it: the schedule and each player's own training.</summary>
public class TrainingOverview
{
    /// <summary>The schedule this week follows, e.g. the template it was built from.</summary>
    public string? Schedule { get; set; }

    /// <summary>This week and the next, Monday to Sunday.</summary>
    public List<TrainingWeek> Weeks { get; set; } = [];

    /// <summary>The sessions a slot can take, by category.</summary>
    public List<TrainingSessionCategory> SessionTypes { get; set; } = [];

    /// <summary>
    /// Every additional focus the squad's players can take. Goalkeepers and outfield players have their own, and
    /// injured players also have rehabilitation; each player's are checked when one is set.
    /// </summary>
    public List<TrainingOption> FocusOptions { get; set; } = [];

    /// <summary>The intensities a player's training can take, and the rest periods he can be given.</summary>
    public List<TrainingOption> IntensityOptions { get; set; } = [];

    public List<PlayerTraining> Players { get; set; } = [];
}
