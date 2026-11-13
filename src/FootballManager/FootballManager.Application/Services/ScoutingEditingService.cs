using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Text;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;

namespace FootballManager.Application.Services;

public interface IScoutingEditingService
{
    /// <summary>Checks a new recruitment focus against the game; creates it only when <paramref name="apply"/> is set.</summary>
    Task<Result<FocusCreation>> CreateFocusAsync(FocusBrief brief, bool apply, CancellationToken cancellationToken = default);

    /// <summary>Removes a recruitment focus by id, only when <paramref name="apply"/> is set.</summary>
    Task<Result<FocusRemoval>> RemoveFocusAsync(int focusId, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Has a scout watch a player: the named one (a scout, or another member of staff the game offers), or any scout
    /// when <paramref name="scout"/> is null. Only when <paramref name="apply"/> is set.
    /// </summary>
    Task<Result<PlayerScoutingChange>> ScoutPlayerAsync(
        int playerId, string? scout, ScoutingDuration duration, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Has a scout watch every player of a team who isn't watched yet, one assignment each, as <see cref="ScoutPlayerAsync"/>
    /// does for one. Only when <paramref name="apply"/> is set.
    /// </summary>
    /// <param name="expectedPlayers">
    /// Needed to apply: how many players the dry run listed, as the user agreed to it. Nothing is sent when the plan no
    /// longer has that many.
    /// </param>
    Task<Result<TeamScoutingChange>> ScoutTeamAsync(
        int teamId, string? scout, ScoutingDuration duration, int? expectedPlayers, bool apply, CancellationToken cancellationToken = default);

    /// <summary>Makes a new shortlist by name, only when <paramref name="apply"/> is set.</summary>
    Task<Result<ShortlistCreation>> CreateShortlistAsync(string name, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a player to one of the user's shortlists, named by <paramref name="shortlist"/> (needed once there are
    /// several), only when <paramref name="apply"/> is set.
    /// </summary>
    Task<Result<PlayerScoutingChange>> ShortlistPlayerAsync(
        int playerId, string? shortlist, ShortlistDuration duration, bool apply, CancellationToken cancellationToken = default);

    /// <summary>Takes a player off the shortlist, only when <paramref name="apply"/> is set.</summary>
    Task<Result<PlayerScoutingChange>> UnshortlistPlayerAsync(int playerId, bool apply, CancellationToken cancellationToken = default);
}

// Changes the user's scouting as the game's screens would, after checking every value against what the game offers
// right now: the areas and ages of the focus wizard, the entries of a player's menu. A dry run checks and says what
// would be done; nothing is written without apply.
public class ScoutingEditingService(IScoutingEditorGameClient editor, IScoutingGameClient scouting) : IScoutingEditingService
{
    // The focus wizard won't go below half a star.
    public const int MinStars = 1;

    // The scouting durations by time, which the menu always lists first; those by matches follow for a player at a club.
    private const int TimeDurations = (int)ScoutingDuration.OneMatch;
    public const int MaxStars = 20;
    public const int MaxNameLength = 120;

    // The "new shortlist" dialog takes up to 100 characters.
    public const int MaxShortlistNameLength = 100;
    private const int SuggestionsShown = 10;

    public async Task<Result<FocusCreation>> CreateFocusAsync(FocusBrief brief, bool apply, CancellationToken cancellationToken = default)
    {
        try
        {
            var areas = editor.GetAreasAsync(cancellationToken);
            var choices = editor.GetFocusChoicesAsync(cancellationToken);
            var focuses = scouting.GetFocusesAsync(cancellationToken);

            await Task.WhenAll(areas, choices, focuses);

            var draft = Check(brief, await areas, await choices, await focuses, out var errors);

            if (errors.Count > 0)
            {
                return Result.Fail<FocusCreation>(errors);
            }

            var creation = new FocusCreation { Applied = apply, Draft = draft };

            if (apply)
            {
                creation.Focus = await editor.CreateFocusAsync(draft, cancellationToken);

                if (creation.Focus is null)
                {
                    creation.Notes.Add("The game created the focus, but its focus list doesn't show it under its new settings yet " +
                                       "(after a removal it can reuse the removed focus's id and keep showing that focus for a while).");
                }
            }

            return Result.Ok(creation);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<FocusCreation>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<FocusCreation>(new ValidationError("game", [exception.Message]));
        }
    }

    public async Task<Result<FocusRemoval>> RemoveFocusAsync(int focusId, bool apply, CancellationToken cancellationToken = default)
    {
        try
        {
            var focuses = await scouting.GetFocusesAsync(cancellationToken);

            if (focuses.FirstOrDefault(f => f.Id == focusId) is not { } focus)
            {
                var known = string.Join(", ", focuses.Select(f => $"{f.Name} (id {f.Id})"));
                return Result.Fail<FocusRemoval>(new ValidationError("focusId",
                    [$"No recruitment focus with id {focusId}. The club's focuses: {(known.Length > 0 ? known : "none")}."]));
            }

            if (apply)
            {
                await editor.RemoveFocusAsync(focusId, cancellationToken);
            }

            return Result.Ok(new FocusRemoval { Applied = apply, Focus = focus });
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<FocusRemoval>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<FocusRemoval>(new ValidationError("game", [exception.Message]));
        }
    }

    public Task<Result<PlayerScoutingChange>> ScoutPlayerAsync(
        int playerId, string? scout, ScoutingDuration duration, bool apply, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(playerId, PlayerMenuAction.AssignScout, apply,
            (player, menu) => ScoutEntry(player, menu, scout, duration), cancellationToken);
    }

    // Plans every player first, from his own menu, so a scout the game doesn't offer stops the run before anything is
    // sent; then sends them one at a time, stopping at the first the game refuses. Applying needs the count the dry
    // run showed: the user agrees to that many assignments, not to whatever the squad holds by then.
    public async Task<Result<TeamScoutingChange>> ScoutTeamAsync(
        int teamId, string? scout, ScoutingDuration duration, int? expectedPlayers, bool apply, CancellationToken cancellationToken = default)
    {
        if (apply && expectedPlayers is null)
        {
            return Result.Fail<TeamScoutingChange>(new ValidationError("expectedPlayers",
                ["Applying needs expectedPlayers: the number of players the dry run listed, which the user agreed to."]));
        }

        try
        {
            if (await editor.GetTeamPlayersAsync(teamId, cancellationToken) is not { } team)
            {
                return Result.Fail<TeamScoutingChange>(new ValidationError("teamId",
                    [$"No team with id {teamId} in the user's league or competitions, nor among the clubs the database search " +
                     "has found (search for it first)."]));
            }

            var change = new TeamScoutingChange { Applied = apply, TeamId = team.TeamId, TeamName = team.TeamName };
            var planned = new List<(ScoutedPlayer Player, List<int> Entry)>();

            change.Skipped.AddRange(team.Players.Where(player => player.IsBeingScouted).Select(player => PlayerMenus.Skip(player, "already being scouted")));

            var candidates = team.Players.Where(player => !player.IsBeingScouted).ToList();
            var menus = await PlayerMenus.AFewAtATimeAsync(
                candidates, PlayerMenus.MenusAtOnce, player => editor.GetMenuAsync(player.Id, PlayerMenuAction.AssignScout, cancellationToken), cancellationToken);

            for (var i = 0; i < candidates.Count; i++)
            {
                var player = candidates[i];
                var menu = menus[i];

                if (menu is null)
                {
                    change.Skipped.Add(PlayerMenus.Skip(player, "the game doesn't offer to scout him"));
                    continue;
                }

                var entry = ScoutEntry(player, menu, scout, duration);

                if (entry.IsFailed)
                {
                    // The scouts the game offers are the same for every player: a scout it doesn't know stops the run.
                    if (entry.HasError<ValidationError>(error => error.PropertyName == "scout"))
                    {
                        return entry.ToResult<TeamScoutingChange>();
                    }

                    change.Skipped.Add(PlayerMenus.Skip(player,
                        string.Join(" ", entry.Errors.OfType<ValidationError>().SelectMany(error => error.ErrorMessages))));
                    continue;
                }

                var picked = PlayerMenus.ConfirmationOf(menu, entry.Value);

                if (picked.IsWarning)
                {
                    change.Skipped.Add(PlayerMenus.Skip(player, $"the game warns against it: {picked.Text}"));
                    continue;
                }

                planned.Add((player, entry.Value));
                change.Players.Add(new PlayerScoutingChange
                {
                    Applied = false,
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    Action = PlayerMenuAction.AssignScout,
                    Choice = PlayerMenus.Label(menu, entry.Value),
                    Confirmation = picked.Text,
                    IsShortlisted = player.IsShortlisted,
                    IsBeingScouted = player.IsBeingScouted
                });
            }

            change.Budget = await scouting.GetBudgetAsync(cancellationToken);

            if (apply)
            {
                if (expectedPlayers != planned.Count)
                {
                    return Result.Fail<TeamScoutingChange>(new ValidationError("expectedPlayers",
                        [$"The user agreed to {expectedPlayers} players, but {planned.Count} would be sent a scout now. " +
                         "Nothing was sent: run the dry run again and show the user the new plan."]));
                }

                await SendAsync(change, planned, cancellationToken);
            }

            return Result.Ok(change);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TeamScoutingChange>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<TeamScoutingChange>(new ValidationError("game", [exception.Message]));
        }
    }

    // Runs each planned entry in turn. The first refusal (the budget spent, say) would come again for the rest: the run
    // stops there and says so, keeping what was already sent.
    private async Task SendAsync(
        TeamScoutingChange change, List<(ScoutedPlayer Player, List<int> Entry)> planned, CancellationToken cancellationToken)
    {
        for (var i = 0; i < planned.Count; i++)
        {
            try
            {
                var after = await editor.RunMenuEntryAsync(planned[i].Player.Id, PlayerMenuAction.AssignScout, planned[i].Entry, cancellationToken);
                change.Players[i].Applied = true;
                change.Players[i].IsBeingScouted = after?.IsBeingScouted;
            }
            catch (Exception exception) when (exception is GameRejectedException or GameUnavailableException)
            {
                var why = exception is GameRejectedException ? "refused to scout" : "stopped answering while scouting";
                change.Notes.Add($"The game {why} {planned[i].Player.Name}: {exception.Message} The run stopped there; " +
                                 $"{change.Players.Count - i} players weren't sent a scout" +
                                 (exception is GameUnavailableException ? $", and whether {planned[i].Player.Name} was isn't known." : "."));
                change.Players.RemoveRange(i, change.Players.Count - i);
                return;
            }
        }
    }

    // The entry sending a scout: any scout, or the one named, for the duration asked. The menu's first entries are any
    // scout, then the people the game lets the user name (scouts, other staff), each with the same durations below.
    private static Result<List<int>> ScoutEntry(ScoutedPlayer player, List<MenuEntry> menu, string? scout, ScoutingDuration duration)
    {
        var anyScout = menu.FirstOrDefault(entry => entry.Children.Count > 0 && entry.Children.All(child => child.CanRun));

        if (scout is null)
        {
            return anyScout is null
                ? Result.Fail<List<int>>(new ValidationError("scout", ["The game offers no 'any scout' entry; name a scout."]))
                : Duration(menu, [menu.IndexOf(anyScout)], (int)duration, Enum.GetValues<ScoutingDuration>().Length, TimeDurations);
        }

        var people = menu
            .Select((entry, index) => (entry, index))
            .Where(top => top.entry.Children.Any(person => person.Children.Count > 0))
            .SelectMany(top => top.entry.Children.Select((person, index) => (Path: new List<int> { top.index, index }, person.Label)))
            .ToList();

        var wanted = GameNames.Normalise(scout);
        var matches = people.Where(person => GameNames.Normalise(person.Label).StartsWith(wanted, StringComparison.Ordinal)).ToList();

        return matches.Count switch
        {
            1 => Duration(menu, matches[0].Path, (int)duration, Enum.GetValues<ScoutingDuration>().Length, TimeDurations),
            0 => Result.Fail<List<int>>(new ValidationError("scout",
                [$"No one called '{scout}' can be sent to watch {player.Name}. The game offers: {string.Join(", ", people.Select(p => p.Label))}."])),
            _ => Result.Fail<List<int>>(new ValidationError("scout",
                [$"'{scout}' matches {string.Join(", ", matches.Select(m => m.Label))}. Give the full name."]))
        };
    }

    public async Task<Result<ShortlistCreation>> CreateShortlistAsync(string name, bool apply, CancellationToken cancellationToken = default)
    {
        var wanted = name.Trim();

        if (wanted.Length == 0 || wanted.Length > MaxShortlistNameLength)
        {
            return Result.Fail<ShortlistCreation>(new ValidationError("name",
                [$"A shortlist's name runs from 1 to {MaxShortlistNameLength} characters."]));
        }

        try
        {
            var lists = await scouting.GetShortlistsAsync(cancellationToken);

            if (lists.Any(list => GameNames.Normalise(list.Name) == GameNames.Normalise(wanted)))
            {
                return Result.Fail<ShortlistCreation>(new ValidationError("name", [$"There's already a shortlist called '{wanted}'."]));
            }

            if (!apply)
            {
                return Result.Ok(new ShortlistCreation { Name = wanted, Shortlists = lists });
            }

            await editor.CreateShortlistAsync(wanted, cancellationToken);

            return Result.Ok(new ShortlistCreation
            {
                Applied = true,
                Name = wanted,
                Created = true,
                Shortlists = await scouting.GetShortlistsAsync(cancellationToken)
            });
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<ShortlistCreation>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<ShortlistCreation>(new ValidationError("game", [exception.Message]));
        }
    }

    public async Task<Result<PlayerScoutingChange>> ShortlistPlayerAsync(
        int playerId, string? shortlist, ShortlistDuration duration, bool apply, CancellationToken cancellationToken = default)
    {
        List<Shortlist> lists;

        try
        {
            lists = await scouting.GetShortlistsAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<PlayerScoutingChange>(new GameUnavailableError(exception.Message));
        }

        return await ChangeAsync(playerId, PlayerMenuAction.AddToShortlist, apply,
            (_, menu) => PickShortlist(menu, lists, shortlist, duration), cancellationToken);
    }

    // The menu's entry lists the shortlists, each with its durations, once there are several; with only the default
    // one, the durations come straight under it.
    internal static Result<List<int>> PickShortlist(List<MenuEntry> menu, List<Shortlist> lists, string? shortlist, ShortlistDuration duration)
    {
        if (menu.Count == 0)
        {
            return Result.Fail<List<int>>(new ValidationError("playerId", ["The game's menu has no entry to shortlist him."]));
        }

        var add = menu[0];
        var durations = Enum.GetValues<ShortlistDuration>().Length;

        if (add.Children.Count == 0)
        {
            return add.CanRun
                ? Result.Ok(new List<int> { 0 })
                : Result.Fail<List<int>>(new ValidationError("playerId", ["The game won't shortlist him."]));
        }

        var names = string.Join(", ", lists.Select(list => $"'{list.Name}'"));

        if (add.Children.All(child => child.Children.Count == 0))
        {
            return shortlist is null || lists.Count == 1 && GameNames.Normalise(lists[0].Name) == GameNames.Normalise(shortlist)
                ? Duration(menu, [0], (int)duration, durations)
                : Result.Fail<List<int>>(new ValidationError("shortlist", [$"No shortlist called '{shortlist}': the user has {names}."]));
        }

        if (shortlist is null)
        {
            return Result.Fail<List<int>>(new ValidationError("shortlist", [$"The user has several shortlists: name one of {names}."]));
        }

        var index = add.Children.FindIndex(child => GameNames.Normalise(child.Label) == GameNames.Normalise(shortlist));

        return index < 0
            ? Result.Fail<List<int>>(new ValidationError("shortlist",
                [$"No shortlist called '{shortlist}' to add him to: the game offers {string.Join(", ", add.Children.Select(child => $"'{child.Label}'"))}."]))
            : Duration(menu, [0, index], (int)duration, durations);
    }

    public Task<Result<PlayerScoutingChange>> UnshortlistPlayerAsync(int playerId, bool apply, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(playerId, PlayerMenuAction.RemoveFromShortlist, apply, (_, menu) =>
            menu.Count > 0 && menu[0].CanRun
                ? Result.Ok(new List<int> { 0 })
                : Result.Fail<List<int>>(new ValidationError("playerId", ["The game's menu has no entry to take him off the shortlist."])),
            cancellationToken);
    }

    // Finds the player, reads the menu the game offers him for the action, picks the entry, and runs it on apply.
    private async Task<Result<PlayerScoutingChange>> ChangeAsync(
        int playerId, PlayerMenuAction action, bool apply, Func<ScoutedPlayer, List<MenuEntry>, Result<List<int>>> pick,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await editor.FindPlayerAsync(playerId, cancellationToken) is not { } player)
            {
                return Result.Fail<PlayerScoutingChange>(new ValidationError("playerId",
                    [$"Player {playerId} isn't in the user's scouting lists (the scouting centre's recommendations, the shortlist, " +
                     "a recruitment focus's players, or the player search's results), which get_scouted_players lists, nor " +
                     "among the players the database search or get_competition_stats has listed: list him there first."]));
            }

            var menu = await editor.GetMenuAsync(playerId, action, cancellationToken);

            if (menu is null)
            {
                return Result.Fail<PlayerScoutingChange>(new ValidationError("playerId", [NotOffered(action, player)]));
            }

            var entry = pick(player, menu);

            if (entry.IsFailed)
            {
                return entry.ToResult<PlayerScoutingChange>();
            }

            var picked = PlayerMenus.ConfirmationOf(menu, entry.Value);

            var change = new PlayerScoutingChange
            {
                Applied = apply,
                PlayerId = playerId,
                PlayerName = player.Name,
                Action = action,
                Choice = PlayerMenus.Label(menu, entry.Value),
                Confirmation = picked.Text,
                ConfirmationIsWarning = picked.IsWarning,
                Budget = action == PlayerMenuAction.AssignScout ? await scouting.GetBudgetAsync(cancellationToken) : null,
                IsShortlisted = player.IsShortlisted,
                IsBeingScouted = player.IsBeingScouted
            };

            if (apply && picked.IsWarning)
            {
                return Result.Fail<PlayerScoutingChange>(new ValidationError("game",
                    [$"The game warns against this, so it wasn't done: {picked.Text}"]));
            }

            if (apply)
            {
                var after = await editor.RunMenuEntryAsync(playerId, action, entry.Value, cancellationToken);
                change.IsShortlisted = after?.IsShortlisted ?? (action == PlayerMenuAction.RemoveFromShortlist ? false : null);
                change.IsBeingScouted = after?.IsBeingScouted;
            }

            return Result.Ok(change);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<PlayerScoutingChange>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<PlayerScoutingChange>(new ValidationError("game", [exception.Message]));
        }
    }

    // The duration below the entry at path, by its place in the game's list, which holds them in a fixed order. The list
    // can stop after its first `shortest` entries: a player without a club has no matches to be watched for.
    private static Result<List<int>> Duration(List<MenuEntry> menu, List<int> path, int index, int expected, int? shortest = null)
    {
        var durations = PlayerMenus.Entry(menu, path).Children;
        var labels = string.Join(", ", durations.Select(d => d.Label));

        if (durations.Count != expected && durations.Count != shortest)
        {
            return Result.Fail<List<int>>(new ValidationError("duration", [$"The game's menu lists other durations than expected: {labels}."]));
        }

        if (index >= durations.Count)
        {
            return Result.Fail<List<int>>(new ValidationError("duration",
                [$"The game only offers {labels} for this player: watching him for a number of matches needs a club's matches."]));
        }

        if (!durations[index].CanRun)
        {
            return Result.Fail<List<int>>(new ValidationError("duration", [$"The game won't take '{durations[index].Label}' for this player."]));
        }

        return Result.Ok<List<int>>([.. path, index]);
    }

    private static string NotOffered(PlayerMenuAction action, ScoutedPlayer player)
    {
        return action switch
        {
            PlayerMenuAction.AddToShortlist when player.IsShortlisted => $"{player.Name} is already on the shortlist.",
            PlayerMenuAction.RemoveFromShortlist when !player.IsShortlisted => $"{player.Name} isn't on the shortlist.",
            _ => $"The game doesn't offer that for {player.Name} right now."
        };
    }

    private static FocusDraft Check(
        FocusBrief brief, List<ScoutingArea> areas, FocusChoices choices, List<RecruitmentFocus> focuses, out List<IError> errors)
    {
        var problems = new List<IError>();

        void Add(string property, string message)
        {
            problems.Add(new ValidationError(property, [message]));
        }

        var name = brief.Name.Trim();

        if (name.Length == 0)
        {
            Add("name", "Give the focus a name.");
        }
        else if (name.Length > MaxNameLength)
        {
            Add("name", $"A focus's name is at most {MaxNameLength} characters.");
        }
        else if (focuses.Any(f => GameNames.Normalise(f.Name) == GameNames.Normalise(name)))
        {
            Add("name", $"The club already has a focus called '{name}'.");
        }

        var picked = new List<ScoutingArea>();

        foreach (var text in brief.Areas)
        {
            var area = FindArea(text, areas);

            if (area.IsFailed)
            {
                problems.AddRange(area.Errors);
            }
            else if (picked.All(a => a.Id != area.Value.Id))
            {
                picked.Add(area.Value);
            }
        }

        var positions = new List<TacticalPosition>();

        foreach (var code in brief.Positions)
        {
            if (!TacticalPosition.TryParse(code, out var position) || !choices.Positions.Contains(position!))
            {
                Add("positions", $"'{code}' isn't a position a focus can ask for. It takes: {string.Join(", ", choices.Positions.Select(p => p.Code))}.");
            }
            else if (!positions.Contains(position!))
            {
                positions.Add(position!);
            }
        }

        if (brief.MinAge is { } min && !choices.MinAges.Contains(min))
        {
            Add("minAge", $"The youngest age a focus can start at is {choices.MinAges.FirstOrDefault()}, the oldest {choices.MinAges.LastOrDefault()}.");
        }

        if (brief.MaxAge is { } max && !choices.MaxAges.Contains(max))
        {
            Add("maxAge", $"A focus's age range ends between {choices.MaxAges.FirstOrDefault()} and {choices.MaxAges.LastOrDefault()}.");
        }

        if (brief is { MinAge: { } from, MaxAge: { } to } && from > to)
        {
            Add("maxAge", "The age range ends before it starts.");
        }

        foreach (var (property, stars) in new[] { ("minAbilityStars", brief.MinAbilityStars), ("minPotentialStars", brief.MinPotentialStars) })
        {
            if (stars is < MinStars or > MaxStars)
            {
                Add(property, $"A focus's stars run {MinStars} to {MaxStars} (half a star to five stars).");
            }
        }

        if (brief is { MinAbilityStars: { } ability, MinPotentialStars: { } potential } && ability > potential)
        {
            Add("minAbilityStars", "The minimum ability can't be above the minimum potential.");
        }

        errors = problems;

        return new FocusDraft
        {
            Name = name,
            Areas = picked,
            Positions = positions,
            MinAge = brief.MinAge,
            MaxAge = brief.MaxAge,
            MinAbilityStars = brief.MinAbilityStars,
            MinPotentialStars = brief.MinPotentialStars
        };
    }

    // "Spain", "nation:Spain", or "Premiership (Scotland)" with its group: by name ignoring case and accents, the
    // kind and group narrowing it down when names repeat.
    private static Result<ScoutingArea> FindArea(string text, List<ScoutingArea> areas)
    {
        var wanted = text.Trim();
        var candidates = areas;
        var colon = wanted.IndexOf(':');

        if (colon > 0 && Enum.TryParse<ScoutingAreaKind>(wanted[..colon].Trim(), ignoreCase: true, out var kind))
        {
            candidates = areas.Where(area => area.Kind == kind).ToList();
            wanted = wanted[(colon + 1)..].Trim();
        }

        var normalised = GameNames.Normalise(wanted);
        var exact = candidates
            .Where(area => GameNames.Normalise(area.Name) == normalised
                           || (area.Group is not null && GameNames.Normalise($"{area.Name} ({area.Group})") == normalised))
            .ToList();

        // Otherwise part of the name, with the group (in brackets) narrowing it down: "Championship (Scotland)".
        var (name, group) = SplitGroup(normalised);
        var close = exact.Count > 0
            ? exact
            : candidates
                .Where(area => name.Length >= 3 && GameNames.Normalise(area.Name).Contains(name))
                .Where(area => group is null || (area.Group is not null && GameNames.Normalise(area.Group) == group))
                .ToList();

        return close.Count switch
        {
            1 => Result.Ok(close[0]),
            0 => Result.Fail<ScoutingArea>(new ValidationError("areas",
                [$"No scouting area called '{text}' (names are in the game's language)."])),
            _ => Result.Fail<ScoutingArea>(new ValidationError("areas",
                [$"'{text}' matches several areas: {string.Join("; ", close.Take(SuggestionsShown).Select(a => a.Display))}" +
                 $"{(close.Count > SuggestionsShown ? "; ..." : "")}. Prefix the kind (nation:, region:, continent:, competition:) " +
                 "or add the group in brackets."]))
        };
    }

    // "championship (ecosse)" into the name and the group in brackets; the group is null without brackets.
    private static (string Name, string? Group) SplitGroup(string text)
    {
        var open = text.LastIndexOf(" (", StringComparison.Ordinal);

        return open > 0 && text.EndsWith(')')
            ? (text[..open].Trim(), text[(open + 2)..^1].Trim())
            : (text, null);
    }
}
