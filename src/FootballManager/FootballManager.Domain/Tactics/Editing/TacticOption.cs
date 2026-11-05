namespace FootballManager.Domain.Tactics.Editing;

/// <summary>
/// A choice the game offers: its key, the game's own English identifier whatever language the game runs in, and the
/// label the game shows for it, in the game's language.
/// </summary>
public sealed record TacticOption(string Key, string? Label);
