using System.Collections.Concurrent;
using FootballManager.Domain.Scouting;

namespace FootballManager.Infrastructure.Clients;

// The focuses removed through the API, kept for the life of the process. The game's focus list keeps a removed focus's
// record under its id, and when it gives that id to a newer focus it goes on showing the removed one for a while: an
// entry that still reads as a focus removed here is that stale record, not a focus of the club.
internal sealed class RemovedFocusLog
{
    private readonly ConcurrentDictionary<int, (string Name, string? Created)> _removed = new();

    public void Add(RecruitmentFocus focus)
    {
        _removed[focus.Id] = (focus.Name, focus.Created);
    }

    /// <summary>
    /// Flags the focuses still showing as one removed here. An id that shows something else again is the game's list
    /// refreshed, and is forgotten.
    /// </summary>
    public void Flag(IEnumerable<RecruitmentFocus> focuses)
    {
        foreach (var focus in focuses)
        {
            if (!_removed.TryGetValue(focus.Id, out var removed))
            {
                continue;
            }

            if (removed.Name == focus.Name && removed.Created == focus.Created)
            {
                focus.ShowsRemovedFocus = true;
            }
            else
            {
                _removed.TryRemove(focus.Id, out _);
            }
        }
    }
}
