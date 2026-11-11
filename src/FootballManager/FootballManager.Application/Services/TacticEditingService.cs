using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;

namespace FootballManager.Application.Services;

public interface ITacticEditingService
{
    Task<Result<TacticOptions>> GetOptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Checks and plans the changes; writes them to the game only when <paramref name="apply"/> is set.</summary>
    Task<Result<TacticEdit>> SetTacticAsync(TacticChanges changes, bool apply, CancellationToken cancellationToken = default);
}

// Edits a tactic slot the way a user would in the game, in the game's order: switch to the slot, then style (which
// resets the mentality and team instructions), mentality, team instructions, formations (which redo the lineup), and
// last the lineup: picks, position moves, roles, player instructions. Each stage plans from what the game holds after
// the one before, so a dry run can only plan what it can see. The first failed write stops the edit.
public class TacticEditingService(ITacticEditorGameClient game) : ITacticEditingService
{
    public const int SlotCount = 3;
    public const int LineupSize = 11;

    /// <summary>The option of a player instruction group that leaves it unset.</summary>
    public const string NoInstruction = "None";

    private static readonly bool[] Phases = [true, false];

    public async Task<Result<TacticOptions>> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await game.GetOptionsAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TacticOptions>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<TacticEdit>> SetTacticAsync(
        TacticChanges changes, bool apply, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = await game.GetOptionsAsync(cancellationToken);
            var tactic = await game.GetTacticAsync(cancellationToken);
            var roles = new RoleLookup(game, cancellationToken);
            var errors = Validate(changes, options, tactic);

            if (errors.Count == 0)
            {
                errors.AddRange(await ValidateRolesAsync(changes, tactic, roles));
            }

            if (errors.Count > 0)
            {
                return Result.Fail<TacticEdit>(errors);
            }

            var edit = new TacticEdit { Slot = changes.Slot, Applied = apply };

            try
            {
                await new EditRun(game, options, roles, edit, apply, cancellationToken).RunAsync(changes, tactic);
            }
            catch (GameUnavailableException exception) when (edit.Steps.Any(step => step.Status == EditStepStatus.Done))
            {
                // Some writes went through before a read between them failed: say which, rather than lose them behind
                // the error.
                edit.Notes.Add($"The game stopped answering partway ({exception.Message}). The steps marked Done were " +
                               "applied, the others weren't: check get_tactics before trying again.");
            }

            return Result.Ok(edit);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TacticEdit>(new GameUnavailableError(exception.Message));
        }
    }

    private static List<IError> Validate(TacticChanges changes, TacticOptions options, EditableTactic tactic)
    {
        var errors = new List<IError>();

        void Add(string property, string message)
        {
            errors.Add(new ValidationError(property, [message]));
        }

        void Check(string property, string? key, IEnumerable<TacticOption> choices)
        {
            var keys = choices.Select(choice => choice.Key).ToList();

            if (key is not null && !keys.Contains(key))
            {
                Add(property, $"Unknown key '{key}'. Use one of: {string.Join(", ", keys)}.");
            }
        }

        if (changes.Slot is < 1 or > SlotCount)
        {
            Add("slot", $"Must be 1 to {SlotCount}.");
        }
        else if (!tactic.Slots.Contains(changes.Slot))
        {
            Add("slot", $"Slot {changes.Slot} holds no tactic; create one there in the game first.");
        }

        Check("style", changes.Style, options.Styles);
        Check("mentality", changes.Mentality, options.Mentalities);

        foreach (var (key, option) in changes.TeamInstructions)
        {
            var instruction = options.TeamInstructions.FirstOrDefault(i => i.Key == key);

            if (instruction is null)
            {
                Add($"teamInstructions.{key}",
                    $"Unknown team instruction. Use one of: {string.Join(", ", options.TeamInstructions.Select(i => i.Key))}.");
            }
            else
            {
                Check($"teamInstructions.{key}", option, instruction.Options);
            }
        }

        foreach (var inPossession in Phases)
        {
            Check($"formation.{PhaseProperty(inPossession)}", changes.Formation?.For(inPossession), options.Formations.For(inPossession) ?? []);
        }

        ValidateLineup(changes.Lineup, options, tactic, Add);
        ValidateBench(changes, tactic, Add);
        return errors;
    }

    private static void ValidateBench(TacticChanges changes, EditableTactic tactic, Action<string, string> add)
    {
        if (changes.Bench is not { } bench)
        {
            return;
        }

        if (bench.Count > tactic.Bench.Count)
        {
            add("bench", $"The bench has {tactic.Bench.Count} places, not {bench.Count}.");
        }

        foreach (var duplicate in bench.GroupBy(id => id).Where(group => group.Count() > 1))
        {
            add("bench", $"Player {duplicate.Key} is listed {duplicate.Count()} times.");
        }

        // The eleven once the lineup is placed: all eleven when given, else the current one.
        var starters = changes.Lineup.Count == LineupSize
            ? changes.Lineup.Select(change => change.PlayerId).ToHashSet()
            : tactic.Lineup.Select(slot => slot.PlayerId).OfType<int>().ToHashSet();

        for (var i = 0; i < bench.Count; i++)
        {
            if (!tactic.Squad.ContainsKey(bench[i]))
            {
                add($"bench[{i}]", $"No first-team player has id {bench[i]} (get_squad lists their ids).");
            }
            else if (starters.Contains(bench[i]))
            {
                add($"bench[{i}]", $"{Name(tactic, bench[i])} starts: a starter can't be on the bench too.");
            }
        }
    }

    private static void ValidateLineup(
        List<LineupChange> lineup, TacticOptions options, EditableTactic tactic, Action<string, string> add)
    {
        if (lineup.Count > LineupSize)
        {
            add("lineup", $"A lineup has {LineupSize} players, not {lineup.Count}.");
            return;
        }

        foreach (var duplicate in lineup.GroupBy(change => change.PlayerId).Where(group => group.Count() > 1))
        {
            add("lineup", $"Player {duplicate.Key} is listed {duplicate.Count()} times.");
        }

        var starters = tactic.Lineup.Select(slot => slot.PlayerId).OfType<int>().ToHashSet();
        var newcomers = lineup.Where(change => !starters.Contains(change.PlayerId)).ToList();

        if (newcomers.Count > 0 && lineup.Count < LineupSize)
        {
            add("lineup", $"{string.Join(", ", newcomers.Select(change => Name(tactic, change.PlayerId)))} " +
                          $"{(newcomers.Count == 1 ? "isn't" : "aren't")} in the starting eleven: to bring players in, list all {LineupSize}.");
        }

        for (var i = 0; i < lineup.Count; i++)
        {
            var change = lineup[i];
            var property = $"lineup[{i}]";

            if (!tactic.Squad.ContainsKey(change.PlayerId))
            {
                add($"{property}.playerId", $"No first-team player has id {change.PlayerId} (get_squad lists their ids).");
            }

            foreach (var inPossession in Phases)
            {
                var phase = PhaseProperty(inPossession);
                var code = change.Position?.For(inPossession);

                if (code is not null && !TacticalPosition.TryParse(code, out _))
                {
                    add($"{property}.position.{phase}", $"Unknown position '{code}'. Use FM's codes: GK, DR, DCL, DC, DCR, DL, WBR, DMR, DM, MR, MC, AMCL, ST, ...");
                }
                else if (code is null && newcomers.Contains(change))
                {
                    add($"{property}.position.{phase}", $"{Name(tactic, change.PlayerId)} comes into the eleven: give their position in both phases.");
                }

                if (change.Role?.For(inPossession) is { } role && !(options.Roles.For(inPossession) ?? []).Contains(role))
                {
                    add($"{property}.role.{phase}", $"Unknown {PhaseName(inPossession)} role '{role}'. The position's roles are listed by get_role_fit.");
                }

                ValidateInstructions(change.Instructions?.For(inPossession), options, inPossession, $"{property}.instructions.{phase}", add);
            }
        }

        foreach (var inPossession in Phases)
        {
            var positions = lineup
                .Select(change => change.Position?.For(inPossession))
                .Select(code => TacticalPosition.TryParse(code, out var position) ? position : null)
                .OfType<TacticalPosition>()
                .ToList();

            foreach (var shared in positions.GroupBy(position => position).Where(group => group.Count() > 1))
            {
                add("lineup", $"{shared.Count()} players are given {shared.Key.Code} {PhaseName(inPossession)}.");
            }

            if (positions.Count(position => position.Position == PitchPosition.GK) > 1)
            {
                add("lineup", $"Only one player can be GK {PhaseName(inPossession)}.");
            }
        }
    }

    private static void ValidateInstructions(
        List<string>? keys, TacticOptions options, bool inPossession, string property, Action<string, string> add)
    {
        if (keys is null)
        {
            return;
        }

        var groups = options.PlayerInstructions.Where(group => group.InPossession == inPossession).ToList();
        var chosen = new List<string>();

        foreach (var key in keys)
        {
            var group = groups.FirstOrDefault(g => g.Options.Any(option => option.Key == key));

            if (group is null)
            {
                add(property, $"Unknown {PhaseName(inPossession)} player instruction '{key}'. get_tactic_options lists them by group.");
            }
            else if (chosen.Contains(group.Key))
            {
                add(property, $"Only one instruction per group: {group.Key} is given twice.");
            }
            else
            {
                chosen.Add(group.Key);
            }
        }
    }

    // Roles depend on the position: checked against the game's own list wherever the position is known before writing.
    private static async Task<List<IError>> ValidateRolesAsync(TacticChanges changes, EditableTactic tactic, RoleLookup roles)
    {
        var errors = new List<IError>();

        // Switching slot or formation changes the positions the players have now.
        var positionsHold = changes.Slot == tactic.ActiveSlot && changes.Formation?.InPossession is null && changes.Formation?.OutOfPossession is null;

        for (var i = 0; i < changes.Lineup.Count; i++)
        {
            var change = changes.Lineup[i];
            var slot = tactic.Lineup.FirstOrDefault(s => s.PlayerId == change.PlayerId);

            foreach (var inPossession in Phases)
            {
                if (change.Role?.For(inPossession) is not { } role)
                {
                    continue;
                }

                var position = TacticalPosition.TryParse(change.Position?.For(inPossession), out var given) ? given
                    : positionsHold ? slot?.Position.For(inPossession)
                    : null;

                if (position is not null && await roles.CheckAsync(change.PlayerId, position, inPossession, role) is { } problem)
                {
                    errors.Add(new ValidationError($"lineup[{i}].role.{PhaseProperty(inPossession)}", [problem]));
                }
            }
        }

        return errors;
    }

    private static string Name(EditableTactic tactic, int playerId)
    {
        return tactic.Squad.GetValueOrDefault(playerId) ?? $"Player {playerId}";
    }

    private static string PhaseProperty(bool inPossession)
    {
        return inPossession ? "inPossession" : "outOfPossession";
    }

    private static string PhaseName(bool inPossession)
    {
        return inPossession ? "in possession" : "out of possession";
    }

    // The roles a position offers a player, asked once per player, position and phase.
    private sealed class RoleLookup(ITacticEditorGameClient game, CancellationToken cancellationToken)
    {
        private readonly Dictionary<(int, PitchPosition, bool), List<RoleRating>> _roles = [];

        public async Task<List<RoleRating>> GetAsync(int playerId, TacticalPosition position, bool inPossession)
        {
            var key = (playerId, position.Position, inPossession);

            if (!_roles.TryGetValue(key, out var roles))
            {
                roles = await game.GetRolesAsync(playerId, position.Position, inPossession, cancellationToken);
                _roles[key] = roles;
            }

            return roles;
        }

        /// <summary>Null when the position offers the role, else why not.</summary>
        public async Task<string?> CheckAsync(int playerId, TacticalPosition position, bool inPossession, string role)
        {
            var roles = await GetAsync(playerId, position, inPossession);

            if (roles.Any(r => r.Key == role))
            {
                return null;
            }

            return roles.Count == 0
                ? $"The game rates player {playerId} in no {PhaseName(inPossession)} role at {position.Code}."
                : $"{position.Code} has no {PhaseName(inPossession)} role '{role}'. It offers: {string.Join(", ", roles.Select(r => r.Key))}.";
        }

        public async Task<string?> RatingAsync(int playerId, TacticalPosition position, bool inPossession, string role)
        {
            var rating = (await GetAsync(playerId, position, inPossession)).FirstOrDefault(r => r.Key == role)?.Rating;
            return rating is null ? null : $"rated {rating.Display}";
        }
    }

    private sealed class EditRun(
        ITacticEditorGameClient game, TacticOptions options, RoleLookup roles, TacticEdit edit, bool apply,
        CancellationToken cancellationToken)
    {
        private bool _stopped;

        public async Task RunAsync(TacticChanges changes, EditableTactic tactic)
        {
            // What a dry run can't see before writing: another slot's settings, and positions after a formation change.
            var settingsUnseen = false;
            var lineupUnseen = false;

            if (changes.Slot != tactic.ActiveSlot)
            {
                await WriteAsync(Step("Switch slot", null, $"slot {tactic.ActiveSlot}", $"slot {changes.Slot}"), async () =>
                {
                    await game.SwitchSlotAsync(changes.Slot, cancellationToken);
                    return null;
                });

                edit.Notes.Add($"Slot {changes.Slot} becomes the tactic the team plays, and stays so. The starting eleven " +
                               "goes with it by slot index, as when switching in the game.");
                tactic = await RefreshAsync(tactic);
                settingsUnseen = lineupUnseen = !apply;
            }

            if (changes.Style is { } style && (settingsUnseen || tactic.Style != style))
            {
                await WriteAsync(Step("Style", null, settingsUnseen ? null : tactic.Style, style),
                    async () => (await game.SetStyleAsync(style, cancellationToken)).After);

                edit.Notes.Add("A new style resets the mentality and every team instruction to the style's own; the " +
                               "ones given here are set after it.");
                tactic = await RefreshAsync(tactic);
                settingsUnseen |= !apply;
            }

            if (changes.Mentality is { } mentality && (settingsUnseen || tactic.Mentality != mentality))
            {
                await WriteAsync(Step("Mentality", null, settingsUnseen ? null : tactic.Mentality, mentality),
                    async () => (await game.SetMentalityAsync(mentality, cancellationToken)).After);
            }

            await WriteAllAsync(changes.TeamInstructions
                .Select(change => (Instruction: change.Key, Option: change.Value, Current: tactic.TeamInstructions.GetValueOrDefault(change.Key)))
                .Where(change => settingsUnseen || change.Current != change.Option)
                .Select(change => Write(
                    Step("Team instruction", change.Instruction, settingsUnseen ? null : change.Current, change.Option),
                    async () => (await game.SetTeamInstructionAsync(change.Instruction, change.Option, cancellationToken)).After))
                .ToList());

            var formationChanged = false;

            foreach (var inPossession in Phases)
            {
                var current = tactic.Formation.For(inPossession);

                if (changes.Formation?.For(inPossession) is { } formation && (settingsUnseen || current != formation))
                {
                    await WriteAsync(Step("Formation", PhaseName(inPossession), settingsUnseen ? null : current, formation),
                        async () => (await game.SetFormationAsync(inPossession, formation, cancellationToken)).After);
                    formationChanged = true;
                }
            }

            if (formationChanged)
            {
                edit.Notes.Add("A formation change makes the game redo the lineup: players change slots and that phase's " +
                               "roles go back to their defaults. The lineup given here is placed after it.");
                tactic = await RefreshAsync(tactic);
                lineupUnseen |= !apply;
            }

            if (changes.Lineup.Count > 0 && lineupUnseen)
            {
                edit.Steps.Add(new TacticEditStep
                {
                    Action = "Lineup",
                    To = $"{changes.Lineup.Count} players",
                    Status = EditStepStatus.Planned,
                    Detail = "Picks, moves, roles and player instructions are planned once the slot and formation are in place."
                });
            }
            else if (changes.Lineup.Count > 0)
            {
                await LineupAsync(changes.Lineup, tactic);
            }

            // Picks into the eleven can move players between it and the bench: the bench is read again after them.
            await BenchAsync(changes.Bench, await RefreshAsync(tactic), lineupUnseen, picksPlanned: !apply && changes.Lineup.Count > 0);
        }

        // Fills the bench's places in order. A pick of a player from elsewhere in the matchday squad swaps him with the
        // place's player, as in the game; one from outside it puts him there and the place's player leaves the squad.
        private async Task BenchAsync(List<int>? bench, EditableTactic tactic, bool lineupUnseen, bool picksPlanned)
        {
            if (bench is null)
            {
                if (tactic.Bench.Count > 0 && tactic.Bench.All(place => place.PlayerId is null))
                {
                    edit.Notes.Add($"The bench is empty: the team would play without substitutes. Name up to {tactic.Bench.Count} with bench.");
                }

                return;
            }

            if (lineupUnseen)
            {
                edit.Steps.Add(new TacticEditStep
                {
                    Action = "Bench",
                    To = $"{bench.Count} players",
                    Status = EditStepStatus.Planned,
                    Detail = "Planned once the lineup is in place."
                });
                return;
            }

            if (picksPlanned)
            {
                edit.Notes.Add("The bench is planned from the matchday squad as it stands: picks into the eleven may move " +
                               "players between it and the bench first.");
            }

            var picks = new List<(TacticEditStep Step, int Index, string? Expected)>();

            for (var i = 0; i < bench.Count; i++)
            {
                var place = tactic.Bench[i];
                var playerId = bench[i];

                if (place.PlayerId == playerId)
                {
                    continue;
                }

                var incoming = tactic.Squad.GetValueOrDefault(playerId);
                var step = Step("Bench", $"place {i + 1}", place.PlayerName ?? "empty", incoming);

                await WriteAsync(step, async () =>
                {
                    await game.PickAsync(place.Index, playerId, cancellationToken);
                    return null;
                });

                picks.Add((step, place.Index, incoming));
            }

            if (picks.Count == 0)
            {
                return;
            }

            tactic = await RefreshAsync(tactic);

            foreach (var (step, index, expected) in picks.Where(p => apply && p.Step.Status == EditStepStatus.Done))
            {
                var held = tactic.Bench.FirstOrDefault(place => place.Index == index)?.PlayerName;

                if (held != expected)
                {
                    step.Status = EditStepStatus.Failed;
                    step.Detail = $"The game holds {held ?? "nobody"} there instead (a trialist can't be picked, say).";
                    _stopped = true;
                }
            }
        }

        private async Task LineupAsync(List<LineupChange> lineup, EditableTactic tactic)
        {
            var slots = Assign(lineup, tactic);

            // Who each slot is for: a dry run's picks aren't made, so the slots still name the players going out.
            var players = tactic.Lineup.ToDictionary(slot => slot.Index, slot => slot.PlayerName ?? $"slot {slot.Index}");

            foreach (var (playerId, index) in slots)
            {
                players[index] = tactic.Squad.GetValueOrDefault(playerId) ?? players[index];
            }

            var picks = await PickAsync(lineup, tactic, slots);

            if (picks.Count > 0)
            {
                edit.Notes.Add("The starting eleven is the team's selection, shared by every slot: the picks change it " +
                               "whichever tactic is played.");
                tactic = await RefreshAsync(tactic);
                Verify(picks, tactic, slot => slot.PlayerName);
            }

            // Where each slot stands once moved; a dry run plans the roles from it.
            var targets = new Dictionary<bool, Dictionary<int, TacticalPosition>>();
            var moved = new HashSet<(int, bool)>();

            foreach (var inPossession in Phases)
            {
                if (Targets(lineup, tactic, slots, inPossession) is not { } target)
                {
                    return;
                }

                targets[inPossession] = target;
                var moves = await MoveAsync(tactic, inPossession, target, players);
                moved.UnionWith(moves.Select(move => (move.Index, inPossession)));
            }

            if (moved.Count > 0)
            {
                tactic = await RefreshAsync(tactic);
            }

            await RolesAsync(lineup, tactic, slots, targets, moved, players);
            await InstructionsAsync(lineup, slots, players);
        }

        // Players already in the eleven keep their slot; a newcomer takes the slot of one who isn't listed, preferably
        // one already where they're to play (the goalkeeper's for a goalkeeper), so fewer moves follow.
        private static Dictionary<int, int> Assign(List<LineupChange> lineup, EditableTactic tactic)
        {
            var slots = new Dictionary<int, int>();
            var open = new List<EditableSlot>();

            foreach (var slot in tactic.Lineup)
            {
                if (slot.PlayerId is { } id && lineup.Any(change => change.PlayerId == id))
                {
                    slots[id] = slot.Index;
                }
                else
                {
                    open.Add(slot);
                }
            }

            foreach (var change in lineup.Where(change => !slots.ContainsKey(change.PlayerId)))
            {
                TacticalPosition.TryParse(change.Position?.InPossession, out var wanted);
                var goalkeeper = wanted?.Position == PitchPosition.GK;

                var slot = open.FirstOrDefault(s => s.Position.InPossession == wanted)
                    ?? open.FirstOrDefault(s => (s.Position.InPossession?.Position == PitchPosition.GK) == goalkeeper)
                    ?? open[0];

                open.Remove(slot);
                slots[change.PlayerId] = slot.Index;
            }

            return slots;
        }

        private async Task<List<(TacticEditStep Step, int Index, string? Expected)>> PickAsync(
            List<LineupChange> lineup, EditableTactic tactic, Dictionary<int, int> slots)
        {
            var picks = new List<(TacticEditStep, int, string?)>();

            // One at a time: each moves a player between the eleven and the bench.
            foreach (var change in lineup)
            {
                var index = slots[change.PlayerId];
                var outgoing = tactic.Lineup[index];

                if (outgoing.PlayerId == change.PlayerId)
                {
                    continue;
                }

                var incoming = tactic.Squad.GetValueOrDefault(change.PlayerId);
                var step = Step("Pick", $"slot {index}", outgoing.PlayerName, incoming);

                await WriteAsync(step, async () =>
                {
                    await game.PickAsync(index, change.PlayerId, cancellationToken);
                    return null;
                });

                picks.Add((step, index, incoming));
            }

            return picks;
        }

        // The position of every slot in a phase once the lineup's are applied; null (and a failed step) when the shape
        // isn't one the game can take.
        private Dictionary<int, TacticalPosition>? Targets(
            List<LineupChange> lineup, EditableTactic tactic, Dictionary<int, int> slots, bool inPossession)
        {
            var target = tactic.Lineup
                .Where(slot => slot.Position.For(inPossession) is not null)
                .ToDictionary(slot => slot.Index, slot => slot.Position.For(inPossession)!);

            foreach (var change in lineup)
            {
                if (TacticalPosition.TryParse(change.Position?.For(inPossession), out var position))
                {
                    target[slots[change.PlayerId]] = position!;
                }
            }

            var goalkeeper = tactic.Lineup.FirstOrDefault(slot => slot.Position.For(inPossession)?.Position == PitchPosition.GK);
            var problem = target.Values.Distinct().Count() < target.Count
                    ? $"Two slots would share a position {PhaseName(inPossession)}: give the players whose spots are taken new ones too."
                : goalkeeper is not null && (target.GetValueOrDefault(goalkeeper.Index)?.Position != PitchPosition.GK
                                             || target.Values.Count(p => p.Position == PitchPosition.GK) > 1)
                    ? $"The goalkeeper's slot stays GK {PhaseName(inPossession)}: put the goalkeeper you want there and the others elsewhere."
                : null;

            if (problem is null)
            {
                return target;
            }

            Fail(Step("Positions", PhaseName(inPossession), null, null), problem);
            return null;
        }

        private async Task<List<(int Index, TacticalPosition To)>> MoveAsync(
            EditableTactic tactic, bool inPossession, Dictionary<int, TacticalPosition> target, Dictionary<int, string> players)
        {
            var current = tactic.Lineup
                .Where(slot => slot.Position.For(inPossession) is not null)
                .ToDictionary(slot => slot.Index, slot => slot.Position.For(inPossession)!);
            var moves = PlanMoves(current, target);
            var at = new Dictionary<int, TacticalPosition>(current);

            // One at a time: each needs its spot free.
            foreach (var (index, to) in moves)
            {
                await WriteAsync(Step("Move", $"{players[index]} ({PhaseName(inPossession)})", at[index].Code, to.Code), async () =>
                {
                    await game.MoveAsync(index, inPossession, to, cancellationToken);
                    return null;
                });

                at[index] = to;
            }

            return moves;
        }

        // Moves to free spots first; when every slot left waits on another's spot, one is parked on a spot nobody
        // holds or wants.
        private static List<(int Index, TacticalPosition To)> PlanMoves(
            Dictionary<int, TacticalPosition> current, Dictionary<int, TacticalPosition> target)
        {
            var at = new Dictionary<int, TacticalPosition>(current);
            var moves = new List<(int, TacticalPosition)>();

            while (at.Keys.Where(index => target.ContainsKey(index) && at[index] != target[index]).ToList() is { Count: > 0 } pending)
            {
                var free = pending.Where(index => !at.ContainsValue(target[index])).Select(index => (int?)index).FirstOrDefault();

                if (free is { } index)
                {
                    at[index] = target[index];
                    moves.Add((index, target[index]));
                    continue;
                }

                var spot = TacticalPosition.All.First(position =>
                    position.Position != PitchPosition.GK && !at.ContainsValue(position) && !target.ContainsValue(position));

                at[pending[0]] = spot;
                moves.Add((pending[0], spot));
            }

            return moves;
        }

        private async Task RolesAsync(
            List<LineupChange> lineup, EditableTactic tactic, Dictionary<int, int> slots,
            Dictionary<bool, Dictionary<int, TacticalPosition>> targets, HashSet<(int, bool)> moved, Dictionary<int, string> players)
        {
            var writes = new List<(TacticEditStep, Func<Task<string?>>)>();

            foreach (var change in lineup)
            {
                var slot = tactic.Lineup[slots[change.PlayerId]];

                foreach (var inPossession in Phases)
                {
                    if (change.Role?.For(inPossession) is not { } role)
                    {
                        continue;
                    }

                    // A dry run doesn't move anyone, and a move can reset the role, so it plans every moved slot's role.
                    var position = apply ? slot.Position.For(inPossession) : targets[inPossession].GetValueOrDefault(slot.Index);
                    var current = slot.Role.For(inPossession);

                    if (position is null || (current == role && (apply || !moved.Contains((slot.Index, inPossession)))))
                    {
                        continue;
                    }

                    var step = Step("Role", $"{players[slot.Index]} ({PhaseName(inPossession)}, {position.Code})", current, role);

                    if (await roles.CheckAsync(change.PlayerId, position, inPossession, role) is { } problem)
                    {
                        Fail(step, problem);
                        return;
                    }

                    step.Detail = await roles.RatingAsync(change.PlayerId, position, inPossession, role);
                    writes.Add(Write(step, async () =>
                        (await game.SetRoleAsync(slot.Index, inPossession, role, cancellationToken)).After));
                }
            }

            await WriteAllAsync(writes);
        }

        // A phase's list replaces all of that phase's instructions: a group it doesn't name goes back to none, except
        // the groups the role or position locks (a defensive midfielder always holds position), which keep their option.
        private async Task InstructionsAsync(List<LineupChange> lineup, Dictionary<int, int> slots, Dictionary<int, string> players)
        {
            var writes = new List<(TacticEditStep, Func<Task<string?>>)>();

            foreach (var change in lineup)
            {
                var index = slots[change.PlayerId];

                foreach (var inPossession in Phases)
                {
                    if (change.Instructions?.For(inPossession) is not { } keys)
                    {
                        continue;
                    }

                    // Instructions belong to the slot, not the player: they stay when players change.
                    var current = await game.GetPlayerInstructionsAsync(index, inPossession, cancellationToken);

                    foreach (var group in options.PlayerInstructions.Where(group => group.InPossession == inPossession))
                    {
                        var asked = group.Options.FirstOrDefault(option => keys.Contains(option.Key))?.Key;
                        var setting = current.GetValueOrDefault(group.Key);
                        var held = setting?.Option ?? NoInstruction;
                        var subject = $"{players[index]} ({PhaseName(inPossession)}): {group.Key}";

                        if (setting?.Locked == true)
                        {
                            if (asked is not null && asked != held)
                            {
                                Fail(Step("Player instruction", subject, held, asked),
                                    $"The role or position sets {group.Key} to {held}; change the role to change it.");
                                return;
                            }

                            continue;
                        }

                        var wanted = asked ?? NoInstruction;

                        if (wanted == held)
                        {
                            continue;
                        }

                        var step = Step("Player instruction", subject, held, wanted);

                        // Clearing a group the list doesn't name goes back to the role's default, which some roles
                        // set (a channel midfielder moves into channels): that's the reset, not a failure.
                        writes.Add(Write(step, async () =>
                        {
                            var after = (await game.SetPlayerInstructionAsync(index, inPossession, group.Key, wanted, cancellationToken)).After;

                            if (asked is not null || after == NoInstruction)
                            {
                                return after;
                            }

                            step.Detail = $"The role keeps {after}.";
                            return null;
                        }));
                    }
                }
            }

            await WriteAllAsync(writes);
        }

        // After a refresh, marks the steps whose slot doesn't hold what they wrote.
        private void Verify(
            List<(TacticEditStep Step, int Index, string? Expected)> steps, EditableTactic tactic, Func<EditableSlot, string?> value)
        {
            foreach (var (step, index, expected) in steps.Where(s => apply && s.Step.Status == EditStepStatus.Done))
            {
                var held = value(tactic.Lineup[index]);

                if (held != expected)
                {
                    step.Status = EditStepStatus.Failed;
                    step.Detail = $"The game holds {held ?? "nothing"} there instead.";
                    _stopped = true;
                }
            }
        }

        private async Task<EditableTactic> RefreshAsync(EditableTactic tactic)
        {
            return apply && !_stopped ? await game.GetTacticAsync(cancellationToken) : tactic;
        }

        private Task WriteAsync(TacticEditStep step, Func<Task<string?>> write)
        {
            return WriteAllAsync([Write(step, write)]);
        }

        // One write at a time: the game drops writes to the same property that arrive together (nine roles sent at once
        // left eight unchanged). Each returns the key the game holds afterwards, or null when it can't say;
        // holding anything other than the step's target is a failure, and stops the edit.
        private async Task WriteAllAsync(IReadOnlyCollection<(TacticEditStep Step, Func<Task<string?>> Write)> writes)
        {
            edit.Steps.AddRange(writes.Select(write => write.Step));

            foreach (var (step, write) in writes)
            {
                if (!apply || _stopped)
                {
                    step.Status = apply ? EditStepStatus.Skipped : EditStepStatus.Planned;
                    continue;
                }

                try
                {
                    var after = await write();
                    var kept = after is not null && step.To is not null && after != step.To;

                    step.Status = kept ? EditStepStatus.Failed : EditStepStatus.Done;

                    if (kept)
                    {
                        step.Detail = $"The game holds {after} instead.";
                    }
                }
                catch (Exception exception) when (exception is GameRejectedException or GameUnavailableException)
                {
                    step.Status = EditStepStatus.Failed;
                    step.Detail = exception.Message;
                }

                _stopped |= step.Status == EditStepStatus.Failed;
            }
        }

        // A step found impossible before writing, in a run or a dry run alike: nothing after it is written.
        private void Fail(TacticEditStep step, string problem)
        {
            step.Status = EditStepStatus.Failed;
            step.Detail = problem;
            edit.Steps.Add(step);
            _stopped = true;
        }

        private static (TacticEditStep, Func<Task<string?>>) Write(TacticEditStep step, Func<Task<string?>> write)
        {
            return (step, write);
        }

        private static TacticEditStep Step(string action, string? subject, string? from, string? to)
        {
            return new TacticEditStep { Action = action, Subject = subject, From = from, To = to };
        }
    }
}
