namespace FootballManager.Domain.Scouting;

/// <summary>How well a player must play the searched positions.</summary>
public enum PositionFamiliarity
{
    Natural,

    /// <summary>Accomplished or better.</summary>
    Accomplished,

    /// <summary>Competent or better.</summary>
    Competent,

    /// <summary>Able to play there at all.</summary>
    Able,

    /// <summary>Unable to play there.</summary>
    Unable
}
