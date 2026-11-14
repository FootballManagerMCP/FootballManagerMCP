using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Clients;

/// <summary>
/// Where a player was found: a path to him through a list holding him, from the user's manager (Human) or the game,
/// with what that list needs to be read: the person search's type (the shortlist, the player search), or the contexts
/// of a competition's stats table (its stat, its stage, the gender of the lists the path goes through).
/// </summary>
internal sealed record PlayerPlace(
    string Root, string Path, int? SearchType = null, IReadOnlyList<KeyValuePair<string, long>>? Contexts = null)
{
    /// <summary>The query with the contexts the place's list needs.</summary>
    public BridgeQuery On(BridgeQuery query)
    {
        var placed = ScoutingBridgeClient.InSearch(query, SearchType);

        foreach (var (name, value) in Contexts ?? [])
        {
            placed = placed.Context(name, value);
        }

        return placed;
    }

    /// <summary>The player himself, read from his root.</summary>
    public BridgeQuery Read()
    {
        return On(Root == GameConstants.GameRoot ? BridgeQuery.FromGame() : BridgeQuery.FromHuman()).Base(Path);
    }
}
