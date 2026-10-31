using FootballManager.Domain.Attributes;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces;

/// <summary>Reads data from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it can't.</summary>
public interface IGameClient
{
    /// <summary>
    /// Returns the user's first-team squad with the given <paramref name="attributes"/> for each player, plus current
    /// and potential ability when <paramref name="includeAbility"/> is set. Only what is asked for is fetched.
    /// </summary>
    Task<Squad> GetSquadAsync(
        IReadOnlyCollection<AttributeDefinition> attributes, bool includeAbility, CancellationToken cancellationToken = default);
}
