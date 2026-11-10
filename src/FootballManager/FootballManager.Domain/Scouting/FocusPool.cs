namespace FootballManager.Domain.Scouting;

/// <summary>A recruitment focus's players.</summary>
public enum FocusPool
{
    /// <summary>The ones its scouts recommend.</summary>
    Recommended,

    /// <summary>The ones its scouts are still assessing.</summary>
    Current,

    /// <summary>The ones that just missed its criteria.</summary>
    NearMiss
}
