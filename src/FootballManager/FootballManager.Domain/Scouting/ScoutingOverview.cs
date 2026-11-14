namespace FootballManager.Domain.Scouting;

/// <summary>The club's scouting at a glance: what the scouting centre holds, the scouts, and the recruitment focuses.</summary>
public class ScoutingOverview
{
    public int? RecommendedPlayers { get; set; }
    public int? HighlyRecommendedPlayers { get; set; }

    /// <summary>Reports the user asked for that are still being written.</summary>
    public int? RequestedReports { get; set; }

    /// <summary>Players on the user's shortlists, every list together.</summary>
    public int? ShortlistedPlayers { get; set; }

    public List<Shortlist> Shortlists { get; set; } = [];

    /// <summary>Every player the club's scouting knows something about.</summary>
    public int? ScoutedPlayers { get; set; }

    public ScoutingBudget? Budget { get; set; }

    public List<Scout> Scouts { get; set; } = [];

    /// <summary>The focuses as the scouting screen lists them, filtered by <see cref="FocusesShown"/>.</summary>
    public List<RecruitmentFocus> Focuses { get; set; } = [];

    public FocusListFilter? FocusesShown { get; set; }
}
