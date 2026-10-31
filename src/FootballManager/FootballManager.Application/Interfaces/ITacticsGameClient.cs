using FootballManager.Domain.Tactics;

namespace FootballManager.Application.Interfaces;

/// <summary>Reads tactics from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it can't.</summary>
public interface ITacticsGameClient
{
    /// <summary>Returns the user's three tactic slots with the game's effectiveness ratings, and the active tactic in full.</summary>
    Task<TeamTactics> GetTacticsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns every role available at <paramref name="position"/> with each squad player's rating in it.</summary>
    Task<RoleFit> GetRoleFitAsync(PitchPosition position, bool inPossession, CancellationToken cancellationToken = default);
}
