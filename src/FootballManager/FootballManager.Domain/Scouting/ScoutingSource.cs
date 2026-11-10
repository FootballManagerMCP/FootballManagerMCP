namespace FootballManager.Domain.Scouting;

/// <summary>Where a list of scouted players comes from.</summary>
public enum ScoutingSource
{
    /// <summary>The scouting centre's recommended players, across every focus.</summary>
    Recommendations,

    /// <summary>The user's shortlist.</summary>
    Shortlist,

    /// <summary>One recruitment focus's players (see <see cref="FocusPool"/>).</summary>
    Focus,

    /// <summary>The results of the game's player search, with the filters its search screen holds.</summary>
    Search
}
