namespace FootballManager.Infrastructure.Clients;

// Where each player of the last page read from the player search stood in its results, kept for the life of the
// process. Reaching a player by his place in the search takes a fraction of a second; finding him by id means reading
// every result, which takes half a minute for a thousand. A player is only looked for after being listed, so his place
// is usually known; if the search has changed since, the place no longer holds him and he's looked for by id.
internal sealed class SearchPositions
{
    private volatile IReadOnlyDictionary<int, int> _positions = new Dictionary<int, int>();

    public void Remember(IReadOnlyList<int?> playerIds)
    {
        var positions = new Dictionary<int, int>();

        for (var position = 0; position < playerIds.Count; position++)
        {
            if (playerIds[position] is { } id)
            {
                positions.TryAdd(id, position);
            }
        }

        _positions = positions;
    }

    public int? Of(int playerId)
    {
        return _positions.TryGetValue(playerId, out var position) ? position : null;
    }
}
