using FootballManager.Domain.Database;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Searches the game's database with its own search box. Throws <see cref="Exceptions.GameUnavailableException"/> when
/// the game can't be reached, and <see cref="Exceptions.GameRejectedException"/> when it won't take the search.
/// </summary>
public interface IDatabaseSearchGameClient
{
    /// <summary>
    /// The first <paramref name="take"/> results for <paramref name="text"/>, in the game's order, of the kinds asked
    /// for (all kinds when none are). The search box shows the text afterwards, as if the user had typed it.
    /// </summary>
    Task<DatabaseSearch> SearchAsync(
        string text, IReadOnlyCollection<DatabaseRecordKind> kinds, int take, CancellationToken cancellationToken = default);
}
