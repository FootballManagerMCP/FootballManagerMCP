using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Changes the user's scouting in the running game the way its screens do: the recruitment focus wizard, and the
/// menu the game offers on a player. Throws <see cref="Exceptions.GameUnavailableException"/> when the game can't be
/// reached, and <see cref="Exceptions.GameRejectedException"/> when it refuses or doesn't take a change.
/// </summary>
public interface IScoutingEditorGameClient
{
    /// <summary>Every area a recruitment focus can cover.</summary>
    Task<List<ScoutingArea>> GetAreasAsync(CancellationToken cancellationToken = default);

    /// <summary>The ages and positions a new focus can take.</summary>
    Task<FocusChoices> GetFocusChoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the focus through the game's wizard and returns it as the game lists it, or null when the game created it
    /// (its count of focuses grew) but its list doesn't show it yet.
    /// </summary>
    Task<RecruitmentFocus?> CreateFocusAsync(FocusDraft draft, CancellationToken cancellationToken = default);

    /// <summary>Removes the focus (one of <see cref="IScoutingGameClient.GetFocusesAsync"/>'s).</summary>
    Task RemoveFocusAsync(int focusId, CancellationToken cancellationToken = default);

    /// <summary>Makes a new shortlist with this name, as the game's "new shortlist" dialog does.</summary>
    Task CreateShortlistAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// A player from one of the user's scouting lists (the scouting centre's recommendations, the shortlist, a
    /// focus's players), or null when none holds him.
    /// </summary>
    Task<ScoutedPlayer?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The players of these ids that the player search's listed results, the scouting lists, the club's own squad
    /// (trialists included) or the game's search box hold, by id; an id none holds is left out. Unlike <see cref="FindPlayerAsync"/>, it doesn't read the whole
    /// player search: only the places its listed results were seen at.
    /// </summary>
    Task<Dictionary<int, ScoutedPlayer>> FindPlayersAsync(IReadOnlyCollection<int> playerIds, CancellationToken cancellationToken = default);

    /// <summary>The menu actions of <see cref="PlayerMenuAction"/> the game offers for the player now.</summary>
    Task<HashSet<PlayerMenuAction>> GetOfferedActionsAsync(int playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// A team's squad, by the team's FM id (a team of the user's competitions, or a club the game's search has listed),
    /// with each player made reachable for <see cref="GetMenuAsync"/>; null for a team neither has listed.
    /// </summary>
    Task<TeamPlayers?> GetTeamPlayersAsync(int teamId, CancellationToken cancellationToken = default);

    /// <summary>The player's menu for <paramref name="action"/>, or null when the game doesn't offer it for him.</summary>
    Task<List<MenuEntry>?> GetMenuAsync(int playerId, PlayerMenuAction action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Picks the menu entry at <paramref name="entry"/> (an index per level, from the top) and returns the player as
    /// the game then holds him.
    /// </summary>
    Task<ScoutedPlayer?> RunMenuEntryAsync(
        int playerId, PlayerMenuAction action, IReadOnlyList<int> entry, CancellationToken cancellationToken = default);
}
