namespace FootballManager.Domain.Scouting;

/// <summary>Which recruitment focuses the game's scouting screen lists, as the user last chose.</summary>
public enum FocusListFilter
{
    /// <summary>Every active focus, started or not (the game's default).</summary>
    Active = 0,
    InProgress = 1,
    Completed = 2
}
