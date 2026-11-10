using System.Collections.Concurrent;
using FootballManager.Domain.Database;

namespace FootballManager.Infrastructure.Clients;

// Where the game's search last listed each player and club, kept for the life of the process. The game has no lookup
// by id, and a record found with the search may be in none of the lists the other tools read (a club outside the
// user's competitions, a player the scouts have never seen): it's reached through the search box's results instead.
internal sealed class SearchedRecords
{
    private readonly ConcurrentDictionary<(DatabaseRecordKind Kind, int Id), SearchedPlace> _places = new();

    public void Remember(DatabaseRecordKind kind, int id, SearchedPlace place)
    {
        _places[(kind, id)] = place;
    }

    public SearchedPlace? Of(DatabaseRecordKind kind, int id)
    {
        return _places.GetValueOrDefault((kind, id));
    }
}
