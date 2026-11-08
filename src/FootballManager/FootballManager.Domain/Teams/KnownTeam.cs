namespace FootballManager.Domain.Teams;

/// <summary>A team the game can be asked about: one of the user's league, or of a competition the user plays in.</summary>
public sealed record KnownTeam(int Id, string Name, string Competition);
