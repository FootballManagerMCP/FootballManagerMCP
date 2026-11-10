namespace FootballManager.Infrastructure.Clients;

/// <summary>
/// Where the game's search listed a record: the text searched for, the result's place in the list, and the record's
/// path under that result (the result's own record, or a person's club).
/// </summary>
internal sealed record SearchedPlace(string Query, int Position, string Path);
