using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Database;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Changes the user's scouting through the plugin, as the game's screens do. A focus is made with the focus wizard:
// its draft is reset, filled in field by field, and confirmed. A player is changed through his own menu: the game
// builds the menu for him, and picking an entry sends the event and payload the game built for it, as a click does.
internal sealed class ScoutingEditorBridgeClient(
    GameBridge bridge,
    IScoutingGameClient scouting,
    RemovedFocusLog removed,
    SearchPositions searchPositions,
    DatabaseSearchBridgeClient databaseSearch,
    TeamsBridgeClient teams,
    CompetitionPlaces competitionPlaces)
    : IScoutingEditorGameClient
{
    // The game shows a change a frame or more after its event.
    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan SettlePoll = TimeSpan.FromMilliseconds(200);

    private const int MaxStarSteps = 20;

    // What a write needs to know of a player, and shows of him after.
    private static readonly string[] PlayerFlags =
        [GameConstants.UniqueId, GameConstants.Name, ScoutingConstants.IsShortlisted, ScoutingConstants.IsBeingScouted];

    // Where each player was found, by id: a path to him through a list holding him (a scouting list, a team's squad, a
    // competition's stats table), from the user's manager (Human) or, for one found through the game's search, the game;
    // with the contexts that list needs.
    private readonly Dictionary<int, PlayerPlace> _players = [];

    public async Task<List<ScoutingArea>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        var areas = await bridge.GetRecordAsync<BridgeAreasDto>(
            BridgeQuery.FromHuman().Base(GameConstants.Team).WithHumanAndTeam().Props([ScoutingConstants.AreasOnTeam]),
            cancellationToken);

        return ScoutingAreaTree.Flatten(areas.Values.Options);
    }

    public async Task<FocusChoices> GetFocusChoicesAsync(CancellationToken cancellationToken = default)
    {
        return (await DraftAsync(cancellationToken)).Adapt<FocusChoices>();
    }

    public Task<RecruitmentFocus?> CreateFocusAsync(FocusDraft draft, CancellationToken cancellationToken = default)
    {
        return WithAllFocusesShownAsync<RecruitmentFocus?>(async () =>
        {
            var before = (await scouting.GetFocusesAsync(cancellationToken)).Select(focus => focus.Id).ToHashSet();
            var count = await FocusCountAsync(cancellationToken);

            // A draft left half filled in (the user's own, or a failed run) would carry over into this one.
            await DraftEventAsync(ScoutingConstants.CancelFocusEvent, cancellationToken);

            try
            {
                await FillAsync(draft, cancellationToken);

                var filled = await DraftAsync(cancellationToken);

                if (filled.IsValid == false)
                {
                    throw new GameRejectedException(string.IsNullOrEmpty(filled.DuplicateName)
                        ? "The game finds the focus incomplete."
                        : $"The game already has a focus called '{filled.DuplicateName}'.");
                }

                await DraftEventAsync(ScoutingConstants.ConfirmFocusEvent, cancellationToken);
            }
            catch
            {
                await DraftEventAsync(ScoutingConstants.CancelFocusEvent, CancellationToken.None);
                throw;
            }

            if (await SettleAsync(() => FocusCountAsync(cancellationToken), now => now > count ? (object)now : null, cancellationToken) is null)
            {
                throw new GameRejectedException("The game didn't create the focus.");
            }

            // The new focus as listed, by its name: after a removal the game can reuse the removed focus's id and keep
            // showing that focus's settings under it for a while.
            await ScoutingInfoEventAsync(ScoutingConstants.RefreshTablesEvent, 0, cancellationToken);

            return await SettleAsync(
                () => scouting.GetFocusesAsync(cancellationToken),
                focuses => focuses.FirstOrDefault(focus => !before.Contains(focus.Id) && focus.Name == draft.Name)
                           ?? focuses.LastOrDefault(focus => focus.Name == draft.Name),
                cancellationToken);
        }, cancellationToken);
    }

    public Task RemoveFocusAsync(int focusId, CancellationToken cancellationToken = default)
    {
        return WithAllFocusesShownAsync(async () =>
        {
            var before = await FocusCountAsync(cancellationToken);
            var focus = (await scouting.GetFocusesAsync(cancellationToken)).FirstOrDefault(f => f.Id == focusId);

            await ScoutingInfoEventAsync(ScoutingConstants.RefreshTablesEvent, 0, cancellationToken);
            await ScoutingInfoEventAsync(ScoutingConstants.RemoveFocusEvent, focusId, cancellationToken);

            if (await SettleAsync(() => FocusCountAsync(cancellationToken), count => count < before ? (object)count : null, cancellationToken) is null)
            {
                throw new GameRejectedException($"The game didn't remove focus {focusId}.");
            }

            await ScoutingInfoEventAsync(ScoutingConstants.DeleteFocusEvent, focusId, cancellationToken);

            // Removed: should the game reuse its id, its list may go on showing this focus under it.
            if (focus is not null)
            {
                removed.Add(focus);
            }

            var gone = await SettleAsync(
                () => scouting.GetFocusesAsync(cancellationToken),
                focuses => focuses.All(focus => focus.Id != focusId) ? focuses : null,
                cancellationToken);

            return gone ?? throw new GameRejectedException($"The game removed focus {focusId} but still lists it.");
        }, cancellationToken);
    }

    // As the "new shortlist" dialog does once its name is typed in: the event on the user's manager, the name its data.
    public async Task CreateShortlistAsync(string name, CancellationToken cancellationToken = default)
    {
        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.FromHuman()
                .Base(ScoutingConstants.Manager)
                .ToWrite("/native", ("event", ScoutingConstants.CreateShortlistEvent), ("data", JsonSerializer.Serialize(name)), ("template", "none")),
            cancellationToken);

        var listed = await SettleAsync(
            () => scouting.GetShortlistsAsync(cancellationToken),
            lists => lists.Any(list => list.Name == name) ? lists : null,
            cancellationToken);

        // The game only acts on it while its shortlist screen is open (as the dialog that sends it is).
        if (listed is null)
        {
            throw new GameRejectedException(
                $"The game didn't make the shortlist '{name}': it only does while its shortlist screen is open in the game. " +
                "Ask the user to open that screen, then try again.");
        }
    }

    public async Task<ScoutedPlayer?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default)
    {
        var lists = await ScoutingListsAsync(cancellationToken);

        // The player search last, and on its own: it can hold a thousand players or more, read in half a minute.
        return (await FindInAsync([playerId], lists, cancellationToken)).GetValueOrDefault(playerId)
               ?? await FindInSearchAsync(playerId, cancellationToken)
               ?? await FindInCompetitionsAsync(playerId, cancellationToken)
               ?? await FindInDatabaseSearchAsync(playerId, cancellationToken)
               ?? (await FindInAsync([playerId], [(ScoutingConstants.PersonSearch, ScoutingConstants.PlayerSearch)], cancellationToken))
               .GetValueOrDefault(playerId);
    }

    public async Task<Dictionary<int, ScoutedPlayer>> FindPlayersAsync(IReadOnlyCollection<int> playerIds, CancellationToken cancellationToken = default)
    {
        // The club's own squad (trialists are in it) and the scouting lists first: they reach a player by his id, where
        // the player search reaches him by his place in its results, which moves as the game updates them.
        List<(string Path, int? SearchType)> lists =
            [($"{TeamConstants.OwnTeam}.{TeamConstants.Players}", null), .. await ScoutingListsAsync(cancellationToken)];

        var found = await FindInAsync(playerIds, lists, cancellationToken);

        // Then where the player search listed the rest, one read each.
        foreach (var id in Missing(playerIds, found))
        {
            if (await FindInSearchAsync(id, cancellationToken) is { } player)
            {
                found[id] = player;
            }
        }

        foreach (var id in Missing(playerIds, found))
        {
            if (await FindInCompetitionsAsync(id, cancellationToken) is { } player)
            {
                found[id] = player;
            }
        }

        foreach (var id in Missing(playerIds, found))
        {
            if (await FindInDatabaseSearchAsync(id, cancellationToken) is { } player)
            {
                found[id] = player;
            }
        }

        // Not the whole player search, as FindPlayerAsync does for one: thousands of results take minutes to read.
        return found;
    }

    private static List<int> Missing(IReadOnlyCollection<int> playerIds, Dictionary<int, ScoutedPlayer> found)
    {
        return playerIds.Where(id => !found.ContainsKey(id)).ToList();
    }

    // The scouting centre's recommendations, the shortlist, and each focus's players.
    private async Task<List<(string Path, int? SearchType)>> ScoutingListsAsync(CancellationToken cancellationToken)
    {
        var lists = new List<(string Path, int? SearchType)>
        {
            (ScoutingConstants.Recommendations, null),
            (ScoutingConstants.PersonSearch, ScoutingConstants.ShortlistSearch)
        };

        foreach (var focus in await scouting.GetFocusesAsync(cancellationToken))
        {
            // A stale entry's players are the removed focus's.
            if (!focus.ShowsRemovedFocus)
            {
                lists.AddRange(Enum.GetValues<FocusPool>().Select(pool => (ScoutingConstants.FocusPlayers(focus.Id, pool), (int?)null)));
            }
        }

        return lists;
    }

    // The player where the game's search box last listed him, searched for again if the box has moved on.
    private async Task<ScoutedPlayer?> FindInDatabaseSearchAsync(int playerId, CancellationToken cancellationToken)
    {
        if (await databaseSearch.LocateAsync(DatabaseRecordKind.Player, playerId, cancellationToken) is not { } path)
        {
            return null;
        }

        var player = await bridge.GetRecordAsync<BridgeScoutedPlayerDto>(
            BridgeQuery.FromGame().Base(path).WithHumanAndTeam().Props(PlayerFlags),
            cancellationToken);

        if (player.Values.Id != playerId)
        {
            return null;
        }

        _players[playerId] = new PlayerPlace(GameConstants.GameRoot, path);
        return player.Values.Adapt<ScoutedPlayer>();
    }

    // The player at his row of a competition's stats table where get_competition_stats last listed him, if it still
    // holds him: tables reorder as matches are played.
    private async Task<ScoutedPlayer?> FindInCompetitionsAsync(int playerId, CancellationToken cancellationToken)
    {
        if (competitionPlaces.Of(playerId) is not { } place || await ReadAtAsync(playerId, place, cancellationToken) is not { } player)
        {
            return null;
        }

        _players[playerId] = place;
        return player;
    }

    // The player at the place in the search where it was last listed, if he's still there.
    private async Task<ScoutedPlayer?> FindInSearchAsync(int playerId, CancellationToken cancellationToken)
    {
        if (searchPositions.Of(playerId) is not { } position)
        {
            return null;
        }

        var path = $"{ScoutingConstants.PersonSearch}[{Format(position)}]";
        BridgeRecordDto<BridgeScoutedPlayerDto> player;

        try
        {
            player = await bridge.GetRecordAsync<BridgeScoutedPlayerDto>(
                ScoutingBridgeClient.InSearch(BridgeQuery.FromHuman(), ScoutingConstants.PlayerSearch)
                    .Base(path)
                    .WithHumanAndTeam()
                    .Props(PlayerFlags),
                cancellationToken);
        }
        catch (GameUnavailableException)
        {
            // The search has fewer results now than that place: he's looked for by id.
            return null;
        }

        if (player.Values.Id != playerId)
        {
            return null;
        }

        _players[playerId] = new PlayerPlace(GameConstants.HumanRoot, path, ScoutingConstants.PlayerSearch);
        return player.Values.Adapt<ScoutedPlayer>();
    }

    // The players of these ids that the lists hold, each in the first list holding him.
    private async Task<Dictionary<int, ScoutedPlayer>> FindInAsync(
        IReadOnlyCollection<int> playerIds, List<(string Path, int? SearchType)> lists, CancellationToken cancellationToken)
    {
        var reads = lists.Select(list => bridge.GetListAsync<BridgeScoutedPlayerDto>(
            ScoutingBridgeClient.InSearch(BridgeQuery.FromHuman(), list.SearchType)
                .List(list.Path)
                .WithHumanAndTeam()
                .Props(PlayerFlags),
            cancellationToken,
            gapsExpected: true)).ToList();

        await Task.WhenAll(reads);

        var wanted = playerIds.ToHashSet();
        var found = new Dictionary<int, ScoutedPlayer>();

        for (var i = 0; i < lists.Count; i++)
        {
            foreach (var player in (await reads[i]).Items)
            {
                if (player.Values.Id is { } id && wanted.Contains(id) && !found.ContainsKey(id))
                {
                    _players[id] = new PlayerPlace(GameConstants.HumanRoot, $"{lists[i].Path}[{GameConstants.UniqueId}={Format(id)}]", lists[i].SearchType);
                    found[id] = player.Values.Adapt<ScoutedPlayer>();
                }
            }
        }

        foreach (var id in wanted.Where(id => !found.ContainsKey(id)))
        {
            _players.Remove(id);
        }

        return found;
    }

    public async Task<HashSet<PlayerMenuAction>> GetOfferedActionsAsync(int playerId, CancellationToken cancellationToken = default)
    {
        var offered = await OfferedAsync(await PlaceAsync(playerId, cancellationToken), cancellationToken);

        return Enum.GetValues<PlayerMenuAction>()
            .Where(action => offered.Contains(ActionKey(ScoutingConstants.ActionIdOf(action))))
            .ToHashSet();
    }

    public async Task<List<MenuEntry>?> GetMenuAsync(int playerId, PlayerMenuAction action, CancellationToken cancellationToken = default)
    {
        var player = await PlaceAsync(playerId, cancellationToken);
        var actionId = ScoutingConstants.ActionIdOf(action);

        if (!(await OfferedAsync(player, cancellationToken)).Contains(ActionKey(actionId)))
        {
            return null;
        }

        var menu = await bridge.GetRecordAsync<BridgeMenuDto>(OnAction(player, actionId).Props([ScoutingConstants.MenuItems]), cancellationToken);
        return menu.Values.Items.Adapt<List<MenuEntry>>();
    }

    // The menu actions the game offers for the player now, keyed as the game lists them ("#<action id>").
    private async Task<HashSet<string>> OfferedAsync(PlayerPlace player, CancellationToken cancellationToken)
    {
        var offered = await bridge.GetRecordAsync<BridgeActionsDto>(
            player.On(BridgeQuery.FromGame())
                .RefList(ScoutingConstants.Records, player.Root, player.Path)
                .WithHumanAndTeam()
                .Props([ScoutingConstants.Actions]),
            cancellationToken);

        return offered.Values.Actions?.Keys.ToHashSet() ?? [];
    }

    private static string ActionKey(uint actionId)
    {
        return $"#{actionId.ToString(CultureInfo.InvariantCulture)}";
    }

    public async Task<ScoutedPlayer?> RunMenuEntryAsync(
        int playerId, PlayerMenuAction action, IReadOnlyList<int> entry, CancellationToken cancellationToken = default)
    {
        if (entry.Count == 0)
        {
            throw new ArgumentException("Name the menu entry to pick.", nameof(entry));
        }

        var player = await PlaceAsync(playerId, cancellationToken);

        // The payload of the entry, where the game keeps it: on a top entry itself, on one below in its RowData.
        var payload = $"@{ScoutingConstants.MenuItems}[{Format(entry[0])}]"
                      + string.Concat(entry.Skip(1).Select(index => $".ChildList.{Format(index)}"))
                      + (entry.Count > 1 ? ".RowData.ActionData" : ".ActionData");

        await bridge.WriteAsync<JsonElement>(
            OnAction(player, ScoutingConstants.ActionIdOf(action)).ToWrite("/native",
                ("event", ScoutingConstants.DoActionEvent), ("field", $"Data:{payload}"), ("template", "none")),
            cancellationToken);

        // The player as the game holds him now; off the shortlist, he may no longer be in the list he was found in.
        // Read where he was found first: looking through every list takes far longer.
        await Task.Delay(SettlePoll, cancellationToken);
        return await ReadAtAsync(playerId, player, cancellationToken) ?? await FindPlayerAsync(playerId, cancellationToken);
    }

    public async Task<TeamPlayers?> GetTeamPlayersAsync(int teamId, CancellationToken cancellationToken = default)
    {
        if (await teams.LocateAsync(teamId, cancellationToken) is not { } team)
        {
            return null;
        }

        var squad = $"{team.Path}.{TeamConstants.Players}";
        var root = team.Root == GameConstants.GameRoot ? BridgeQuery.FromGame() : BridgeQuery.FromHuman();

        var players = await bridge.GetListAsync<BridgeScoutedPlayerDto>(
            root.List(squad).WithHumanAndTeam().Props(PlayerFlags), cancellationToken, gapsExpected: true);

        var found = new List<ScoutedPlayer>();

        foreach (var player in players.Items)
        {
            if (player.Values.Id is { } id)
            {
                _players[id] = new PlayerPlace(team.Root, $"{squad}[{GameConstants.UniqueId}={Format(id)}]");
                found.Add(player.Values.Adapt<ScoutedPlayer>());
            }
        }

        return new TeamPlayers { TeamId = teamId, TeamName = team.Name, Players = found };
    }

    // The player at the place he was found, or null when he's no longer there.
    private async Task<ScoutedPlayer?> ReadAtAsync(int playerId, PlayerPlace place, CancellationToken cancellationToken)
    {
        try
        {
            var player = await bridge.GetRecordAsync<BridgeScoutedPlayerDto>(
                place.Read().WithHumanAndTeam().Props(PlayerFlags),
                cancellationToken);

            return player.Values.Id == playerId ? player.Values.Adapt<ScoutedPlayer>() : null;
        }
        catch (GameUnavailableException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    private async Task FillAsync(FocusDraft draft, CancellationToken cancellationToken)
    {
        await SetAsync(OnDraft(), GameConstants.Name, Text(draft.Name), cancellationToken);

        if (draft.MinAge is { } minAge)
        {
            await SetAsync(OnDraft(), ScoutingConstants.FocusMinAge, ListItem(ScoutingConstants.MinAgeChoices, minAge), cancellationToken);
        }

        if (draft.MaxAge is { } maxAge)
        {
            await SetAsync(OnDraft(), ScoutingConstants.FocusMaxAge, ListItem(ScoutingConstants.MaxAgeChoices, maxAge), cancellationToken);
        }

        foreach (var area in draft.Areas)
        {
            // From the user's manager: the area dropdown is the team's, the draft is under ScoutingInfo.
            await SetAsync(
                BridgeQuery.FromHuman().WithHumanAndTeam(),
                ScoutingConstants.DraftAreaSelection,
                $"@{ScoutingConstants.Areas}[{ScoutingConstants.AreaIdField}={area.Id.ToString(CultureInfo.InvariantCulture)}]",
                cancellationToken);
        }

        if (draft.Positions.Count > 0)
        {
            await SetAsync(OnDraft(), ScoutingConstants.AnyPosition, "false", cancellationToken);

            // Picking a box ticks it, or unticks it if it's ticked: the box goes as the game holds it.
            foreach (var position in draft.Positions)
            {
                var mask = PositionMasks.Of(position).ToString(CultureInfo.InvariantCulture);

                await bridge.WriteAsync<JsonElement>(
                    OnDraft().ToWrite("/native",
                        ("event", ScoutingConstants.SelectPositionEvent),
                        ("field", $"Data:@{ScoutingConstants.FocusPositions}[Position={mask}]"),
                        ("template", "none")),
                    cancellationToken);
            }
        }

        await StarsAsync(draft, cancellationToken);

        var filled = await DraftAsync(cancellationToken);

        if (filled.AreaCount != draft.Areas.Count)
        {
            throw new GameRejectedException($"The game took {filled.AreaCount ?? 0} of the {draft.Areas.Count} areas.");
        }

        var ticked = filled.Positions.Count(box => box.Ticked is > 0);

        if (ticked != draft.Positions.Count)
        {
            throw new GameRejectedException($"The game ticked {ticked} of the {draft.Positions.Count} positions.");
        }
    }

    // A threshold moves a step at a time, and ability can't go above potential: potential is raised first and lowered
    // last.
    private async Task StarsAsync(FocusDraft draft, CancellationToken cancellationToken)
    {
        if (draft.MinPotentialStars is { } potential && potential > await StarsAsync(potential: true, cancellationToken))
        {
            await StepAsync(ScoutingConstants.PotentialStepEvent, potential, potential: true, cancellationToken);
        }

        if (draft.MinAbilityStars is { } ability)
        {
            await StepAsync(ScoutingConstants.AbilityStepEvent, ability, potential: false, cancellationToken);
        }

        if (draft.MinPotentialStars is { } lower && lower < await StarsAsync(potential: true, cancellationToken))
        {
            await StepAsync(ScoutingConstants.PotentialStepEvent, lower, potential: true, cancellationToken);
        }
    }

    private async Task StepAsync(string stepEvent, int target, bool potential, CancellationToken cancellationToken)
    {
        for (var step = 0; step < MaxStarSteps; step++)
        {
            var current = await StarsAsync(potential, cancellationToken);

            if (current == target)
            {
                return;
            }

            await bridge.WriteAsync<JsonElement>(
                OnDraft().ToWrite("/native", ("event", stepEvent), ("field", $"Data:{(current < target ? "true" : "false")}"), ("template", "none")),
                cancellationToken);

            if (await StarsAsync(potential, cancellationToken) == current)
            {
                throw new GameRejectedException(
                    $"The game won't take the minimum {(potential ? "potential" : "ability")} stars past {current} (asked: {target}).");
            }
        }

        throw new GameRejectedException($"The minimum {(potential ? "potential" : "ability")} stars didn't reach {target}.");
    }

    private async Task<int> StarsAsync(bool potential, CancellationToken cancellationToken)
    {
        var draft = await DraftAsync(cancellationToken);
        return (potential ? draft.MinPotential : draft.MinAbility)?.MaxValue ?? 0;
    }

    private async Task<BridgeDraftFocusDto> DraftAsync(CancellationToken cancellationToken)
    {
        var draft = await bridge.GetRecordAsync<BridgeDraftFocusDto>(
            OnDraft().Props([
                GameConstants.Name, ScoutingConstants.IsValid, ScoutingConstants.DuplicateName, ScoutingConstants.AreaCount,
                ScoutingConstants.FocusPositions, ScoutingConstants.FocusMinAbility, ScoutingConstants.FocusMinPotential,
                ScoutingConstants.MinAgeChoices, ScoutingConstants.MaxAgeChoices
            ]),
            cancellationToken);

        return draft.Values;
    }

    private Task ScoutingInfoEventAsync(string scoutingEvent, int data, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<JsonElement>(
            OnScoutingInfo().ToWrite("/native", ("event", scoutingEvent), ("field", $"Data:{Format(data)}"), ("template", "none")),
            cancellationToken);
    }

    private async Task<int> FocusCountAsync(CancellationToken cancellationToken)
    {
        var count = await bridge.GetRecordAsync<BridgeFocusCountDto>(
            BridgeQuery.FromHuman().WithHumanAndTeam().Props([ScoutingConstants.FocusCountFromHuman]), cancellationToken);

        return count.Values.Count ?? throw new GameUnavailableException("The game didn't say how many focuses the club has.");
    }

    // The wizard's confirm and cancel carry no data.
    private Task DraftEventAsync(string draftEvent, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<JsonElement>(
            OnDraft().ToWrite("/native", ("event", draftEvent), ("data", "null"), ("template", "none")),
            cancellationToken);
    }

    // Shows every active focus while work looks for one, and puts the user's own choice of list back after: the game's
    // own removal switches the screen to the focuses in progress.
    private async Task<T> WithAllFocusesShownAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken)
    {
        var shown = await FocusListTypeAsync(cancellationToken);

        if (shown is { } type && type != ScoutingConstants.AllFocusesListType)
        {
            await SetAsync(OnScoutingInfo(), ScoutingConstants.FocusListType, Format(ScoutingConstants.AllFocusesListType), cancellationToken);
        }

        try
        {
            return await work();
        }
        finally
        {
            if (shown is { } wanted && await FocusListTypeAsync(CancellationToken.None) != wanted)
            {
                await SetAsync(OnScoutingInfo(), ScoutingConstants.FocusListType, Format(wanted), CancellationToken.None);
            }
        }
    }

    private async Task<int?> FocusListTypeAsync(CancellationToken cancellationToken)
    {
        return (await bridge.GetRecordAsync<BridgeFocusListTypeDto>(
            OnScoutingInfo().Props([ScoutingConstants.FocusListType]), cancellationToken)).Values.ListType;
    }

    private Task<BridgeWriteResultDto> SetAsync(BridgeQuery target, string path, string value, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<BridgeWriteResultDto>(target.ToWrite("/set", ("path", path), ("value", value)), cancellationToken);
    }

    private async Task<PlayerPlace> LocateAsync(int playerId, CancellationToken cancellationToken)
    {
        if (!_players.ContainsKey(playerId) && await FindPlayerAsync(playerId, cancellationToken) is null)
        {
            throw new GameRejectedException(
                $"Player {playerId} isn't in the user's scouting lists, nor among the players the database search or a competition's stats have listed.");
        }

        return _players[playerId];
    }

    // Where the player is now. A place in the player search's results is checked first: the game reorders them as it
    // updates them (trialists arriving, say), and a stale place would reach a neighbour. He's looked for again then.
    private async Task<PlayerPlace> PlaceAsync(int playerId, CancellationToken cancellationToken)
    {
        var place = await LocateAsync(playerId, cancellationToken);

        if (!IsSearchPlace(place) || await ReadAtAsync(playerId, place, cancellationToken) is not null)
        {
            return place;
        }

        _players.Remove(playerId);

        return await FindPlayerAsync(playerId, cancellationToken) is not null
            ? _players[playerId]
            : throw new GameRejectedException(
                $"Player {playerId} is no longer where the player search listed him, nor in another list: list the search again.");
    }

    // A place in the player search by index, as its listed results were seen, rather than by the player's id.
    private static bool IsSearchPlace(PlayerPlace place)
    {
        return place.SearchType == ScoutingConstants.PlayerSearch
               && !place.Path.Contains($"[{GameConstants.UniqueId}=", StringComparison.Ordinal);
    }

    // The menu action for the player: game.Action, picked by its id, for the player as its only record.
    private static BridgeQuery OnAction(PlayerPlace player, uint actionId)
    {
        return player.On(BridgeQuery.FromGame())
            .Base(ScoutingConstants.Action)
            .Context(ScoutingConstants.ActionId, actionId)
            .RefList(ScoutingConstants.Records, player.Root, player.Path)
            .WithHumanAndTeam();
    }

    private static BridgeQuery OnDraft()
    {
        return BridgeQuery.FromHuman().Base(ScoutingConstants.Draft).WithHumanAndTeam();
    }

    private static BridgeQuery OnScoutingInfo()
    {
        return BridgeQuery.FromHuman().Base(ScoutingConstants.ScoutingInfo).WithHumanAndTeam();
    }

    private static async Task<T?> SettleAsync<TRead, T>(Func<Task<TRead>> read, Func<TRead, T?> done, CancellationToken cancellationToken)
        where T : class
    {
        var deadline = DateTime.UtcNow + SettleTimeout;

        while (true)
        {
            if (done(await read()) is { } result)
            {
                return result;
            }

            if (DateTime.UtcNow > deadline)
            {
                return null;
            }

            await Task.Delay(SettlePoll, cancellationToken);
        }
    }

    private static string ListItem(string list, int value)
    {
        return $"@{list}[PropertyValue={Format(value)}]";
    }

    // A text literal, as the plugin reads one: in single quotes.
    private static string Text(string value)
    {
        return $"'{value}'";
    }

    private static string Format(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
