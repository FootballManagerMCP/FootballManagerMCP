using System.Collections.Concurrent;

namespace FootballManager.Infrastructure.Clients;

// Where a competition's stats screen last listed each player: his row in a stat's table, kept for the life of the
// process, so scouting and shortlisting reach a player found with get_competition_stats. Tables reorder as matches are
// played, so a row is checked to still hold the player before it's used.
internal sealed class CompetitionPlaces
{
    private readonly ConcurrentDictionary<int, PlayerPlace> _places = new();

    public void Remember(int playerId, PlayerPlace place)
    {
        _places[playerId] = place;
    }

    public PlayerPlace? Of(int playerId)
    {
        return _places.TryGetValue(playerId, out var place) ? place : null;
    }
}
