using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads and writes the active tactic in the running game, by key (the game's English identifiers). Writes reach only
/// the active slot. Throws <see cref="Exceptions.GameUnavailableException"/> when the game can't be reached, and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses a write.
/// </summary>
public interface ITacticEditorGameClient
{
    /// <summary>Everything a tactic can be set to, with what the active tactic holds now.</summary>
    Task<TacticOptions> GetOptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>The active tactic by key: its settings, its eleven slots, and the squad.</summary>
    Task<EditableTactic> GetTacticAsync(CancellationToken cancellationToken = default);

    /// <summary>The roles <paramref name="position"/> offers, with the player's rating in each.</summary>
    Task<List<RoleRating>> GetRolesAsync(int playerId, PitchPosition position, bool inPossession, CancellationToken cancellationToken = default);

    /// <summary>A lineup slot's player instruction in each group of a phase, by group key.</summary>
    Task<Dictionary<string, PlayerInstructionSetting>> GetPlayerInstructionsAsync(
        int index, bool inPossession, CancellationToken cancellationToken = default);

    /// <summary>Makes <paramref name="slot"/> (1 to 3) the tactic the team plays.</summary>
    Task SwitchSlotAsync(int slot, CancellationToken cancellationToken = default);

    Task<WriteOutcome> SetStyleAsync(string style, CancellationToken cancellationToken = default);

    Task<WriteOutcome> SetMentalityAsync(string mentality, CancellationToken cancellationToken = default);

    Task<WriteOutcome> SetTeamInstructionAsync(string instruction, string option, CancellationToken cancellationToken = default);

    /// <summary>The game then redoes the lineup: players change slots and the phase's roles reset.</summary>
    Task<WriteOutcome> SetFormationAsync(bool inPossession, string formation, CancellationToken cancellationToken = default);

    /// <summary>Moves lineup slot <paramref name="index"/> to a spot no other slot holds in that phase.</summary>
    Task MoveAsync(int index, bool inPossession, TacticalPosition position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Puts a player in matchday squad place <paramref name="index"/>: 0-10 the eleven, from 11 the bench. A player
    /// already in the matchday squad swaps places with the place's player; one from outside it takes the place.
    /// </summary>
    Task PickAsync(int index, int playerId, CancellationToken cancellationToken = default);

    Task<WriteOutcome> SetRoleAsync(int index, bool inPossession, string role, CancellationToken cancellationToken = default);

    Task<WriteOutcome> SetPlayerInstructionAsync(
        int index, bool inPossession, string group, string option, CancellationToken cancellationToken = default);
}
