namespace FootballManager.Domain.Scouting;

/// <summary>
/// A place a recruitment focus can send scouts: a competition, a nation, a region or a continent, named in the game's
/// display language. Names repeat across kinds ("France" the nation, and the group of France's competitions).
/// </summary>
public sealed record ScoutingArea(long Id, string Name, ScoutingAreaKind Kind, string? Group)
{
    /// <summary>"Nation: Spain", or "Competition: Ligue 1 (France)".</summary>
    public string Display => Group is null || Kind is ScoutingAreaKind.Continent
        ? $"{Kind}: {Name}"
        : $"{Kind}: {Name} ({Group})";
}
