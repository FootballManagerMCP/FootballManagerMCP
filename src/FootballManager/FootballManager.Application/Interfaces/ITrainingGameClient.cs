using FootballManager.Domain.Training;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads and changes the team's training in the running game. Throws <see cref="Exceptions.GameUnavailableException"/>
/// when the game can't be reached, and <see cref="Exceptions.GameRejectedException"/> when it refuses a change.
/// </summary>
public interface ITrainingGameClient
{
    /// <summary>This week's and next week's schedule, the session types, and each player's own training.</summary>
    Task<TrainingOverview> GetTrainingAsync(CancellationToken cancellationToken = default);

    /// <summary>Sets a player's additional focus (-1 for none), as his focus dropdown does.</summary>
    Task SetFocusAsync(int playerId, long focus, CancellationToken cancellationToken = default);

    /// <summary>Sets a player's training intensity, or rests him, as his intensity dropdown does.</summary>
    Task SetIntensityAsync(int playerId, long intensity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the session at <paramref name="slot"/> on <paramref name="date"/> (one of the read weeks' days), which
    /// holds <paramref name="current"/>, as editing the session's card in the calendar does.
    /// </summary>
    Task SetSessionAsync(string date, int slot, int current, int session, CancellationToken cancellationToken = default);
}
