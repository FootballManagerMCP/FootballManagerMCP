using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Services;

public interface ITrialsService
{
    /// <summary>
    /// Offers each player a trial at the user's club for <paramref name="weeks"/> weeks, through his own menu in the
    /// game, as the user would one by one. Only when <paramref name="apply"/> is set.
    /// </summary>
    /// <param name="expectedPlayers">
    /// Needed to apply: how many players the dry run listed, as the user agreed to it. Nothing is sent when the plan no
    /// longer has that many.
    /// </param>
    Task<Result<TrialChanges>> OfferAsync(
        IReadOnlyList<int> playerIds, int weeks, int? expectedPlayers, bool apply, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the trial of each player on trial at the user's club, and withdraws the offer to each player who hasn't
    /// answered it yet, through his own menu in the game. Only when <paramref name="apply"/> is set.
    /// </summary>
    /// <param name="expectedPlayers">As for <see cref="OfferAsync"/>.</param>
    Task<Result<TrialChanges>> EndAsync(
        IReadOnlyList<int> playerIds, int? expectedPlayers, bool apply, CancellationToken cancellationToken = default);
}

// Trials go through each player's menu: the game builds it for him and offers only what fits where he stands, a trial
// (an entry per length, one to four weeks), the withdrawal of an offer he hasn't answered, or the end of his trial,
// each with the confirmation it would show. A free agent may accept at once and join the club for the trial. A dry run
// reads the menus and says what would be sent; nothing is written without apply.
public class TrialsService(IScoutingEditorGameClient editor) : ITrialsService
{
    public const int MinWeeks = 1;
    public const int MaxWeeks = 4;

    // About a second each to send: fifty stay well within a tool call.
    public const int MaxPlayers = 50;

    // The menu shows a change a frame or more after it's sent.
    private const int StateChecks = 5;
    private static readonly TimeSpan StatePoll = TimeSpan.FromMilliseconds(300);

    // The game can leave a read unanswered while it's busy (a trialist arriving, say); one more try after a pause.
    private static readonly TimeSpan RetryPause = TimeSpan.FromSeconds(1);

    public Task<Result<TrialChanges>> OfferAsync(
        IReadOnlyList<int> playerIds, int weeks, int? expectedPlayers, bool apply, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(playerIds, weeks, expectedPlayers, apply,
            player => PlanOfferAsync(player, weeks, cancellationToken),
            state => state != TrialState.None,
            "offered a trial", cancellationToken);
    }

    public Task<Result<TrialChanges>> EndAsync(
        IReadOnlyList<int> playerIds, int? expectedPlayers, bool apply, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(playerIds, null, expectedPlayers, apply,
            player => PlanEndAsync(player, cancellationToken),
            state => state == TrialState.None,
            "released from a trial or an offer", cancellationToken);
    }

    // Plans every player from his own menu, a few at a time, then sends the changes one at a time.
    private async Task<Result<TrialChanges>> ChangeAsync(
        IReadOnlyList<int> playerIds,
        int? weeks,
        int? expectedPlayers,
        bool apply,
        Func<ScoutedPlayer, Task<(PlayerMenuAction Action, List<MenuEntry>? Menu, List<int>? Entry, string? Skip, TrialState State)>> plan,
        Func<TrialState, bool> reached,
        string done,
        CancellationToken cancellationToken)
    {
        var ids = playerIds.Distinct().ToList();
        var errors = Check(ids, weeks, expectedPlayers, apply);

        if (errors.Count > 0)
        {
            return Result.Fail<TrialChanges>(errors);
        }

        try
        {
            var found = await editor.FindPlayersAsync(ids, cancellationToken);
            var players = ids.Where(found.ContainsKey).Select(id => found[id]).ToList();
            var changes = new TrialChanges { Applied = apply, Weeks = weeks, NotFound = ids.Where(id => !found.ContainsKey(id)).ToList() };

            var plans = await PlayerMenus.AFewAtATimeAsync(players, PlayerMenus.MenusAtOnce, plan, cancellationToken);
            var planned = new List<(ScoutedPlayer Player, PlayerMenuAction Action, List<int> Entry)>();

            foreach (var (player, step) in players.Zip(plans))
            {
                if (step.Skip is { } reason)
                {
                    changes.Skipped.Add(PlayerMenus.Skip(player, reason));
                    continue;
                }

                var picked = PlayerMenus.ConfirmationOf(step.Menu!, step.Entry!);

                if (picked.IsWarning)
                {
                    changes.Skipped.Add(PlayerMenus.Skip(player, $"the game warns against it: {picked.Text}"));
                    continue;
                }

                planned.Add((player, step.Action, step.Entry!));
                changes.Players.Add(new PlayerScoutingChange
                {
                    Applied = false,
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    Action = step.Action,
                    Choice = PlayerMenus.Label(step.Menu!, step.Entry!),
                    Confirmation = picked.Text,
                    IsShortlisted = player.IsShortlisted,
                    Trial = step.State
                });
            }

            if (apply)
            {
                if (expectedPlayers != planned.Count)
                {
                    return Result.Fail<TrialChanges>(new ValidationError("expectedPlayers",
                        [$"The user agreed to {expectedPlayers} players, but {planned.Count} would be {done} now. " +
                         "Nothing was sent: run the dry run again and show the user the new plan."]));
                }

                await SendAsync(changes, planned, reached, done, cancellationToken);
            }

            return Result.Ok(changes);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TrialChanges>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<TrialChanges>(new ValidationError("game", [exception.Message]));
        }
    }

    private static List<IError> Check(List<int> ids, int? weeks, int? expectedPlayers, bool apply)
    {
        var errors = new List<IError>();

        if (ids.Count == 0)
        {
            errors.Add(new ValidationError("playerIds", ["List the players."]));
        }
        else if (ids.Count > MaxPlayers)
        {
            errors.Add(new ValidationError("playerIds", [$"At most {MaxPlayers} players at a time (asked: {ids.Count}); split them up."]));
        }

        if (weeks is < MinWeeks or > MaxWeeks)
        {
            errors.Add(new ValidationError("weeks", [$"A trial lasts {MinWeeks} to {MaxWeeks} weeks, the lengths the game offers."]));
        }

        if (apply && expectedPlayers is null)
        {
            errors.Add(new ValidationError("expectedPlayers",
                ["Applying needs expectedPlayers: the number of players the dry run listed, which the user agreed to."]));
        }

        return errors;
    }

    // The entry for a trial of that length: the trial menu's one top entry holds the lengths, one week to four in that
    // order. Or why he can't be offered one.
    private async Task<(PlayerMenuAction Action, List<MenuEntry>? Menu, List<int>? Entry, string? Skip, TrialState State)> PlanOfferAsync(
        ScoutedPlayer player, int weeks, CancellationToken cancellationToken)
    {
        const PlayerMenuAction action = PlayerMenuAction.OfferTrial;
        var offered = await editor.GetOfferedActionsAsync(player.Id, cancellationToken);
        var state = StateOf(offered);

        if (!offered.Contains(action))
        {
            return (action, null, null, state switch
            {
                TrialState.OnTrial => "he's on trial at the club already",
                TrialState.OfferWaiting => "an offer to him is already waiting for his answer",
                _ => "the game doesn't offer him a trial (he may be on trial at another club, or one of the club's own players)"
            }, state);
        }

        if (await editor.GetMenuAsync(player.Id, action, cancellationToken) is not { Count: > 0 } menu)
        {
            return (action, null, null, "the game doesn't offer him a trial", state);
        }

        var lengths = menu[0].Children;

        if (lengths.Count != MaxWeeks)
        {
            return (action, null, null, $"the game's trial menu lists other lengths than expected: {string.Join(", ", lengths.Select(l => l.Label))}", state);
        }

        return lengths[weeks - 1].CanRun
            ? (action, menu, [0, weeks - 1], null, state)
            : (action, null, null, $"the game won't take '{lengths[weeks - 1].Label}' for him", state);
    }

    // The end of his trial if he's on one, the withdrawal of the offer if he hasn't answered it: a single entry each.
    private async Task<(PlayerMenuAction Action, List<MenuEntry>? Menu, List<int>? Entry, string? Skip, TrialState State)> PlanEndAsync(
        ScoutedPlayer player, CancellationToken cancellationToken)
    {
        var state = StateOf(await editor.GetOfferedActionsAsync(player.Id, cancellationToken));

        if (state == TrialState.None)
        {
            return (PlayerMenuAction.EndTrial, null, null, "he's neither on trial at the club nor waiting on an offer", state);
        }

        var action = state == TrialState.OnTrial ? PlayerMenuAction.EndTrial : PlayerMenuAction.WithdrawOffer;

        return await editor.GetMenuAsync(player.Id, action, cancellationToken) is [{ CanRun: true }, ..] menu
            ? (action, menu, [0], null, state)
            : (action, null, null, "the game's menu has no entry to do it", state);
    }

    // Sends each planned change in turn. The first refusal would likely come again for the rest: the run stops there
    // and says so, keeping what was already sent.
    private async Task SendAsync(
        TrialChanges changes,
        List<(ScoutedPlayer Player, PlayerMenuAction Action, List<int> Entry)> planned,
        Func<TrialState, bool> reached,
        string done,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < planned.Count; i++)
        {
            var (player, action, entry) = planned[i];
            var change = changes.Players[i];

            try
            {
                var after = await RunAsync(player.Id, action, entry, reached, cancellationToken);

                change.Applied = true;
                change.IsShortlisted = after?.IsShortlisted ?? change.IsShortlisted;
                change.Trial = await StateAfterAsync(player.Id, reached, cancellationToken);

                if (!reached(change.Trial.Value))
                {
                    changes.Notes.Add($"The game still shows {player.Name} as {change.Trial} after the change was sent.");
                }
            }
            catch (GameRejectedException exception)
            {
                changes.Notes.Add($"The game refused it for {player.Name}: {exception.Message} The run stopped there; " +
                                  $"{changes.Players.Count - i} players weren't {done}.");
                changes.Players.RemoveRange(i, changes.Players.Count - i);
                return;
            }
        }
    }

    // Runs the entry, and once more after a pause if the game didn't take it, unless it took after all.
    private async Task<ScoutedPlayer?> RunAsync(
        int playerId, PlayerMenuAction action, List<int> entry, Func<TrialState, bool> reached, CancellationToken cancellationToken)
    {
        try
        {
            return await editor.RunMenuEntryAsync(playerId, action, entry, cancellationToken);
        }
        catch (GameRejectedException)
        {
            await Task.Delay(RetryPause, cancellationToken);

            if (reached(StateOf(await editor.GetOfferedActionsAsync(playerId, cancellationToken))))
            {
                return null;
            }

            return await editor.RunMenuEntryAsync(playerId, action, entry, cancellationToken);
        }
    }

    // Where he stands once the menu shows the change, or as it last showed him.
    private async Task<TrialState> StateAfterAsync(int playerId, Func<TrialState, bool> reached, CancellationToken cancellationToken)
    {
        var state = TrialState.None;

        for (var check = 0; check < StateChecks; check++)
        {
            state = StateOf(await editor.GetOfferedActionsAsync(playerId, cancellationToken));

            if (reached(state))
            {
                return state;
            }

            await Task.Delay(StatePoll, cancellationToken);
        }

        return state;
    }

    private static TrialState StateOf(HashSet<PlayerMenuAction> offered)
    {
        return offered.Contains(PlayerMenuAction.EndTrial) ? TrialState.OnTrial
            : offered.Contains(PlayerMenuAction.WithdrawOffer) ? TrialState.OfferWaiting
            : TrialState.None;
    }
}
