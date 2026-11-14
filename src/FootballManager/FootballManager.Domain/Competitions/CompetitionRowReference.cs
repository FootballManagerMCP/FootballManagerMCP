namespace FootballManager.Domain.Competitions;

/// <summary>Where a player sits on a competition's stats screen: a stat's table and his row in it.</summary>
public sealed record CompetitionRowReference(CompetitionStat Stat, int RowIndex);
