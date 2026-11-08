using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Tactics;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Edits the active tactic through the plugin, the way the tactics screen's own controls do: each setting is the
// screen's binding or event, given the game's id for the key asked for. Every write reads the value back, so the
// answer says what the game holds, not what was sent.
internal sealed class TacticEditorBridgeClient(
    GameBridge bridge, GameVocabulary vocabulary, TeamInstructionReader teamInstructionReader) : ITacticEditorGameClient
{
    private const int SlotCount = 3;
    private const int LineupSize = 11;
    private const string NoInstruction = "None";

    // A slot switch shows a few frames after its event.
    private static readonly TimeSpan SwitchTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan SwitchPoll = TimeSpan.FromMilliseconds(250);

    private Task<BridgeListDto<BridgeRoleFitPlayerDto>>? _squad;

    public async Task<TacticOptions> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        var keys = vocabulary.KeysAsync(cancellationToken);
        var playerInstructions = vocabulary.PlayerInstructionsAsync(cancellationToken);
        var settings = SettingsAsync(cancellationToken);
        var teamInstructions = teamInstructionReader.ReadAsync(cancellationToken);
        var withBall = FormationChoiceAsync(inPossession: true, cancellationToken);
        var withoutBall = FormationChoiceAsync(inPossession: false, cancellationToken);

        await Task.WhenAll(keys, playerInstructions, settings, teamInstructions, withBall, withoutBall);

        var names = await keys;
        var current = await settings;

        return new TacticOptions
        {
            ActiveSlot = (current.CurrentSlot ?? 0) + 1,
            Style = Key(names.Styles, Unwrap(current.Style)?.PropertyValue),
            Styles = Choices(current.StyleOptions, names.Styles),
            Mentality = Key(names.Mentalities, current.Mentality),
            Mentalities = Choices(current.MentalityOptions, names.Mentalities),
            Formation = new ByPhase<string>
            {
                InPossession = Key(names.Formations, (await withBall).Formation),
                OutOfPossession = Key(names.Formations, (await withoutBall).Formation)
            },
            Formations = new ByPhase<List<TacticOption>>
            {
                InPossession = Choices(Leaves((await withBall).Options), names.Formations),
                OutOfPossession = Choices(Leaves((await withoutBall).Options), names.Formations)
            },
            TeamInstructions = await teamInstructions,
            PlayerInstructions = (await playerInstructions).Where(group => group.Group is not null).Adapt<List<PlayerInstructionGroup>>(),
            Roles = new ByPhase<List<string>>
            {
                InPossession = names.InPossessionRoles.Values.ToList(),
                OutOfPossession = names.OutOfPossessionRoles.Values.ToList()
            }
        };
    }

    public async Task<EditableTactic> GetTacticAsync(CancellationToken cancellationToken = default)
    {
        // Edits go to the user's current team's tactic: make sure it's theirs (reading another team can switch it).
        await OwnTeam.RestoreAsync(bridge);

        var keys = vocabulary.KeysAsync(cancellationToken);
        var settings = SettingsAsync(cancellationToken);
        var instructions = teamInstructionReader.ReadAsync(cancellationToken);
        var squad = SquadAsync(cancellationToken);
        var formationWithBall = FormationAsync(inPossession: true, cancellationToken);
        var formationWithoutBall = FormationAsync(inPossession: false, cancellationToken);
        var withBall = LineupAsync(inPossession: true, cancellationToken);
        var withoutBall = LineupAsync(inPossession: false, cancellationToken);

        await Task.WhenAll(keys, settings, instructions, squad, formationWithBall, formationWithoutBall, withBall, withoutBall);

        var names = await keys;
        var current = await settings;
        bool?[] valid = [current.Slot1Valid, current.Slot2Valid, current.Slot3Valid];
        var inPossession = (await withBall).Items;
        var outOfPossession = (await withoutBall).Items;

        return new EditableTactic
        {
            ActiveSlot = (current.CurrentSlot ?? 0) + 1,
            Slots = Enumerable.Range(1, SlotCount).Where(slot => valid[slot - 1] == true).ToList(),
            Style = Key(names.Styles, Unwrap(current.Style)?.PropertyValue),
            Mentality = Key(names.Mentalities, current.Mentality),
            Formation = new ByPhase<string>
            {
                InPossession = Key(names.Formations, await formationWithBall),
                OutOfPossession = Key(names.Formations, await formationWithoutBall)
            },
            TeamInstructions = (await instructions).ToDictionary(instruction => instruction.Key, instruction => instruction.Current),

            // Both phases answer in lineup order, one row per slot.
            Lineup = Enumerable.Range(0, Math.Min(inPossession.Count, outOfPossession.Count))
                .Select(index => Slot(index, inPossession[index].Values, outOfPossession[index].Values, names))
                .ToList(),
            Squad = (await squad).Items
                .Where(player => player.Values.Id is not null)
                .GroupBy(player => player.Values.Id!.Value)
                .ToDictionary(group => group.Key, group => group.First().Values.Name ?? string.Empty)
        };
    }

    public async Task<List<RoleRating>> GetRolesAsync(
        int playerId, PitchPosition position, bool inPossession, CancellationToken cancellationToken = default)
    {
        var squad = (await SquadAsync(cancellationToken)).Items;
        var listIndex = squad.FindIndex(player => player.Values.Id == playerId);

        if (listIndex < 0)
        {
            throw new GameRejectedException($"No first-team player has id {playerId}.");
        }

        // The player's role table at the position, as the game's role fit screens read it.
        var answer = await bridge.GetListAsync<BridgeRoleFitPlayerDto>(
            BridgeQuery.FromHuman()
                .List(TacticConstants.SquadPath)
                .Index(listIndex)
                .Context(TacticConstants.PositionMask, PositionMasks.Of(position))
                .Context(TacticConstants.InPossession, inPossession)
                .WithHumanAndTeam()
                .Props([TacticConstants.RoleTable]),
            cancellationToken,
            gapsExpected: true);

        var names = await vocabulary.KeysAsync(cancellationToken);

        return (answer.Items.FirstOrDefault()?.Values.Roles ?? [])
            .Where(row => row.TacticsRoleId is not null)
            .BuildAdapter()
            .AddParameters(MappingConfig.RoleKeysParameter, names.Roles(inPossession))
            .AdaptToType<List<RoleRating>>();
    }

    public async Task<Dictionary<string, PlayerInstructionSetting>> GetPlayerInstructionsAsync(
        int index, bool inPossession, CancellationToken cancellationToken = default)
    {
        var groups = vocabulary.PlayerInstructionsAsync(cancellationToken);
        var values = bridge.GetListAsync<BridgeInstructionValueDto>(
            OnTactic()
                .Context(TacticConstants.TeamSelectionIndex, index)
                .Context(TacticConstants.InPossession, inPossession)
                .Each(TacticConstants.PlayerInstructionGroupId, 0, TacticConstants.PlayerInstructionGroupCount - 1)
                .Props([TacticConstants.SelectedPlayerInstruction, TacticConstants.PlayerInstructionLocked]),
            cancellationToken,
            gapsExpected: true);

        await Task.WhenAll(groups, values);

        // The rows come in group id order.
        var held = (await values).Items;

        return (await groups)
            .Where(group => group.Group is not null && group.GroupId is not null && InPhase(group, inPossession))
            .ToDictionary(group => group.Group!, group =>
            {
                var value = held.ElementAtOrDefault(group.GroupId!.Value)?.Values;
                return new PlayerInstructionSetting(PlayerInstructionKey(group, value?.PlayerInstruction), value?.PlayerInstructionLocked == true);
            });
    }

    public async Task SwitchSlotAsync(int slot, CancellationToken cancellationToken = default)
    {
        await bridge.WriteAsync<JsonElement>(
            OnTacticForWrite().ToWrite(
                "/native", ("event", TacticConstants.SwitchSlotEvent), ("field", $"Data:{slot - 1}"), ("template", "none")),
            cancellationToken);

        var deadline = DateTime.UtcNow + SwitchTimeout;

        while ((await bridge.GetRecordAsync<BridgeTacticSettingsDto>(
                   OnTactic().Props([TacticConstants.CurrentSlot]), cancellationToken)).Values.CurrentSlot != slot - 1)
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new GameRejectedException($"The game didn't switch to slot {slot}.");
            }

            await Task.Delay(SwitchPoll, cancellationToken);
        }
    }

    public async Task<WriteOutcome> SetStyleAsync(string style, CancellationToken cancellationToken = default)
    {
        var names = (await vocabulary.KeysAsync(cancellationToken)).Styles;
        var id = IdOf(names, style, "style");

        return Outcome(await SetAsync(OnTacticForWrite(), TacticConstants.Style,
            $"@{TacticConstants.StyleOptions}[PropertyValue={id}]", cancellationToken), names);
    }

    public async Task<WriteOutcome> SetMentalityAsync(string mentality, CancellationToken cancellationToken = default)
    {
        var names = (await vocabulary.KeysAsync(cancellationToken)).Mentalities;
        var id = IdOf(names, mentality, "mentality");

        return Outcome(await SetAsync(OnTacticForWrite(), TacticConstants.MentalityDropdown,
            $"@{TacticConstants.MentalityOptions}[PropertyValue={id}]", cancellationToken), names);
    }

    public async Task<WriteOutcome> SetTeamInstructionAsync(string instruction, string option, CancellationToken cancellationToken = default)
    {
        var entry = (await vocabulary.TeamInstructionsAsync(cancellationToken)).FirstOrDefault(i => i.Instruction == instruction)
            ?? throw new GameRejectedException($"The game has no team instruction '{instruction}'.");
        var choice = entry.Options.FirstOrDefault(o => o.Option == option)?.Id
            ?? throw new GameRejectedException($"{instruction} has no option '{option}'.");

        var result = await SetAsync(
            OnTacticForWrite().Context(TacticConstants.TeamInstructionId, entry.TeamInstructionId ?? -1),
            TacticConstants.SelectedTeamInstruction, Format(choice), cancellationToken);

        return new WriteOutcome(
            TeamInstructionReader.OptionKey(entry, ValueOf(result.Before)), TeamInstructionReader.OptionKey(entry, ValueOf(result.After)));
    }

    public async Task<WriteOutcome> SetFormationAsync(bool inPossession, string formation, CancellationToken cancellationToken = default)
    {
        var names = (await vocabulary.KeysAsync(cancellationToken)).Formations;
        var id = IdOf(names, formation, "formation");

        return Outcome(await SetAsync(OnTacticForWrite().Context(TacticConstants.InPossession, inPossession),
            TacticConstants.Formation, Format(id), cancellationToken), names);
    }

    public async Task MoveAsync(int index, bool inPossession, TacticalPosition position, CancellationToken cancellationToken = default)
    {
        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.Write("/move",
            [
                ("index", Format(index)), ("to", Format(PositionMasks.Of(position))), ("inPossession", inPossession ? "true" : "false")
            ]),
            cancellationToken);
    }

    public async Task PickAsync(int index, int playerId, CancellationToken cancellationToken = default)
    {
        // The plugin names a player by their place in the game's database.
        var player = (await SquadAsync(cancellationToken)).Items.FirstOrDefault(p => p.Values.Id == playerId)?.Source?.Index
            ?? throw new GameRejectedException($"No first-team player has id {playerId}.");

        await bridge.WriteAsync<JsonElement>(
            BridgeQuery.Write("/pick", [("index", Format(index)), ("player", Format(player))]),
            cancellationToken);
    }

    public async Task<WriteOutcome> SetRoleAsync(int index, bool inPossession, string role, CancellationToken cancellationToken = default)
    {
        var names = (await vocabulary.KeysAsync(cancellationToken)).Roles(inPossession);
        var mask = TacticKeys.IdOf(names, role) ?? throw new GameRejectedException($"The game has no role '{role}'.");

        // The role card's own binding; masks are unsigned, and out-of-possession ones need all 64 bits.
        var result = await SetAsync(
            OnTacticForWrite()
                .Context(TacticConstants.InPossession, inPossession)
                .Context(TacticConstants.TeamSelectionIndex, index),
            TacticConstants.PositionRole, $"u:{Format(mask)}", cancellationToken);

        return new WriteOutcome(Key(names, ValueOf(result.Before)), Key(names, ValueOf(result.After)));
    }

    public async Task<WriteOutcome> SetPlayerInstructionAsync(
        int index, bool inPossession, string group, string option, CancellationToken cancellationToken = default)
    {
        var entry = (await vocabulary.PlayerInstructionsAsync(cancellationToken)).FirstOrDefault(g => g.Group == group)
            ?? throw new GameRejectedException($"The game has no player instruction group '{group}'.");
        var choice = option == NoInstruction
            ? 0UL
            : entry.Options.FirstOrDefault(o => o.Option is not null && TacticKeys.PlayerInstructionKey(o.Option) == option)?.Id
              ?? throw new GameRejectedException($"{group} has no instruction '{option}'.");

        var result = await SetAsync(
            OnTacticForWrite()
                .Context(TacticConstants.InPossession, inPossession)
                .Context(TacticConstants.TeamSelectionIndex, index)
                .Context(TacticConstants.PlayerInstructionGroupId, entry.GroupId ?? -1),
            TacticConstants.SelectedPlayerInstruction, Format(choice), cancellationToken);

        return new WriteOutcome(PlayerInstructionKey(entry, ValueOf(result.Before)), PlayerInstructionKey(entry, ValueOf(result.After)));
    }

    private static BridgeQuery OnTactic()
    {
        return BridgeQuery.OnActiveTactic();
    }

    private static BridgeQuery OnTacticForWrite()
    {
        return BridgeQuery.OnActiveTactic(forWrite: true);
    }

    private Task<BridgeWriteResultDto> SetAsync(BridgeQuery target, string path, string value, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<BridgeWriteResultDto>(target.ToWrite("/set", ("path", path), ("value", value)), cancellationToken);
    }

    private async Task<BridgeTacticSettingsDto> SettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await bridge.GetRecordAsync<BridgeTacticSettingsDto>(
            OnTactic().Props([
                TacticConstants.CurrentSlot, TacticConstants.Slot1Valid, TacticConstants.Slot2Valid, TacticConstants.Slot3Valid,
                TacticConstants.Style, TacticConstants.StyleOptions, TacticConstants.MentalityValue, TacticConstants.MentalityOptions
            ]),
            cancellationToken);

        return settings.Values;
    }

    private Task<BridgeFormationChoiceDto> FormationChoiceAsync(bool inPossession, CancellationToken cancellationToken)
    {
        return ValuesAsync<BridgeFormationChoiceDto>(
            OnTactic()
                .Context(TacticConstants.InPossession, inPossession)
                .Props([TacticConstants.Formation, TacticConstants.FormationOptions]),
            cancellationToken);
    }

    private async Task<long?> FormationAsync(bool inPossession, CancellationToken cancellationToken)
    {
        var formation = await ValuesAsync<BridgeFormationChoiceDto>(
            OnTactic().Context(TacticConstants.InPossession, inPossession).Props([TacticConstants.Formation]),
            cancellationToken);

        return formation.Formation;
    }

    private Task<BridgeListDto<BridgeLineupSlotDto>> LineupAsync(bool inPossession, CancellationToken cancellationToken)
    {
        return bridge.GetListAsync<BridgeLineupSlotDto>(
            OnTactic()
                .Context(TacticConstants.InPossession, inPossession)
                .Each(TacticConstants.TeamSelectionIndex, 0, LineupSize - 1)
                .Props([
                    TacticConstants.LineupPlayerId, TacticConstants.LineupPlayerName, TacticConstants.PositionValue,
                    TacticConstants.RoleValue
                ]),
            cancellationToken);
    }

    // The first team, with each player's place in the game's database (Source.Index), read once per request.
    private Task<BridgeListDto<BridgeRoleFitPlayerDto>> SquadAsync(CancellationToken cancellationToken)
    {
        return _squad ??= bridge.GetListAsync<BridgeRoleFitPlayerDto>(
            BridgeQuery.FromHuman().List(TacticConstants.SquadPath).Props([GameConstants.UniqueId, GameConstants.Name]),
            cancellationToken);
    }

    private async Task<TValues> ValuesAsync<TValues>(BridgeQuery query, CancellationToken cancellationToken) where TValues : new()
    {
        return (await bridge.GetRecordAsync<TValues>(query, cancellationToken)).Values;
    }

    private static EditableSlot Slot(int index, BridgeLineupSlotDto withBall, BridgeLineupSlotDto withoutBall, TacticKeys names)
    {
        return new EditableSlot
        {
            Index = index,
            PlayerId = withBall.PlayerId,
            PlayerName = withBall.PlayerName,
            Position = new ByPhase<TacticalPosition>
            {
                InPossession = withBall.Position is { } mask ? PositionMasks.ToPosition(mask) : null,
                OutOfPossession = withoutBall.Position is { } maskOut ? PositionMasks.ToPosition(maskOut) : null
            },
            Role = new ByPhase<string>
            {
                InPossession = Key(names.InPossessionRoles, withBall.Role),
                OutOfPossession = Key(names.OutOfPossessionRoles, withoutBall.Role)
            }
        };
    }

    // A dropdown's options as keys and labels; options the game's enum doesn't name are left out.
    private static List<TacticOption> Choices(IEnumerable<BridgeOptionDto>? options, IReadOnlyDictionary<long, string> names)
    {
        return (options ?? [])
            .Select(Unwrap)
            .OfType<BridgeOptionDto>()
            .Where(option => option.PropertyValue is { } id && names.ContainsKey(id))
            .DistinctBy(option => option.PropertyValue)
            .Select(option => new TacticOption(names[option.PropertyValue!.Value], option.DisplayValue))
            .ToList();
    }

    // The formations themselves: the groups' entries and the recommendations, not the group headers or the card link
    // (both wrapped in RowData).
    private static IEnumerable<BridgeOptionDto> Leaves(IEnumerable<BridgeOptionDto>? rows)
    {
        return (rows ?? []).SelectMany(row => row.ChildList is { } children ? Leaves(children)
            : row.RowData is null ? [row]
            : []);
    }

    private static BridgeOptionDto? Unwrap(BridgeOptionDto? option)
    {
        return option?.RowData ?? option;
    }

    private static bool InPhase(BridgePlayerInstructionGroupDto group, bool inPossession)
    {
        return TacticConstants.OutOfPossessionInstructionGroups.Contains(group.Group) != inPossession;
    }

    private static string? PlayerInstructionKey(BridgePlayerInstructionGroupDto group, ulong? id)
    {
        if (id is null)
        {
            return null;
        }

        var option = group.Options.FirstOrDefault(o => o.Id == id)?.Option;
        return id == 0 ? NoInstruction : option is null ? Format(id.Value) : TacticKeys.PlayerInstructionKey(option);
    }

    private static WriteOutcome Outcome(BridgeWriteResultDto result, IReadOnlyDictionary<long, string> names)
    {
        return new WriteOutcome(Key(names, (long?)ValueOf(result.Before)), Key(names, (long?)ValueOf(result.After)));
    }

    // A number, or a dropdown option's value.
    private static ulong? ValueOf(JsonElement? value)
    {
        return value switch
        {
            { ValueKind: JsonValueKind.Number } number when number.TryGetUInt64(out var id) => id,
            { ValueKind: JsonValueKind.Object } option when option.TryGetProperty("PropertyValue", out var inner) => ValueOf(inner),
            _ => null
        };
    }

    private static string? Key(IReadOnlyDictionary<long, string> names, long? id)
    {
        return id is { } value ? names.GetValueOrDefault(value) : null;
    }

    private static string? Key(IReadOnlyDictionary<ulong, string> names, ulong? id)
    {
        return id is { } value ? names.GetValueOrDefault(value) : null;
    }

    private static long IdOf(IReadOnlyDictionary<long, string> names, string key, string what)
    {
        return TacticKeys.IdOf(names, key) ?? throw new GameRejectedException($"The game has no {what} '{key}'.");
    }

    private static string Format(long value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string Format(ulong value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
