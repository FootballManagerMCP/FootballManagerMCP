namespace FootballManager.Domain.Scouting;

/// <summary>Players from one of the user's scouting lists, best graded first.</summary>
public class ScoutedPlayerList
{
    public ScoutingSource Source { get; set; }

    /// <summary>The focus the players come from, when <see cref="Source"/> is a focus.</summary>
    public RecruitmentFocus? Focus { get; set; }

    /// <summary>Which of the focus's players, when <see cref="Source"/> is a focus.</summary>
    public FocusPool? Pool { get; set; }

    /// <summary>The game's player search as its screen holds it, when <see cref="Source"/> is the search.</summary>
    public PlayerSearchStatus? Search { get; set; }

    /// <summary>How many players the list holds; more than <see cref="Players"/> when a limit cut it short.</summary>
    public int Total { get; set; }

    public List<ScoutedPlayer> Players { get; set; } = [];

    /// <summary>True when hidden data was asked for but the game masked all of it (no In-Game Editor).</summary>
    public bool HiddenDataMasked { get; set; }
}
