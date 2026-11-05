using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Effectiveness;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Tactics;
using FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the user's tactics through the MCPFM plugin, the way the game's tactics and tactical effectiveness screens do.
// Nothing in the game is switched or changed: the inactive slots are read through their effectiveness screens.
internal sealed class TacticsBridgeClient(
    GameBridge bridge, GameVocabulary vocabulary, TeamInstructionReader teamInstructionReader) : ITacticsGameClient
{
    private const int SlotCount = 3;
    private const int LineupSize = 11;

    public async Task<TeamTactics> GetTacticsAsync(CancellationToken cancellationToken = default)
    {
        var overview = await bridge.GetRecordAsync<BridgeTacticOverviewDto>(
            OnTactic().Props(TacticConstants.Overview), cancellationToken);

        var settings = overview.Values;

        var keys = vocabulary.KeysAsync(cancellationToken);
        var team = bridge.GetRecordAsync<BridgeHumanDto>(
            BridgeQuery.FromHuman().Props([GameConstants.TeamName]), cancellationToken);
        var inPossessionFormation = FormationAsync(inPossession: true, cancellationToken);
        var outOfPossessionFormation = FormationAsync(inPossession: false, cancellationToken);
        var instructions = TeamInstructionsAsync(settings, cancellationToken);
        var lineup = LineupAsync(cancellationToken);
        var slots = SlotsAsync(settings, cancellationToken);

        await Task.WhenAll(keys, team, inPossessionFormation, outOfPossessionFormation, instructions, lineup, slots);

        var names = await keys;
        var withBall = await inPossessionFormation;
        var withoutBall = await outOfPossessionFormation;
        var style = settings.Style is null ? null : Unwrap(settings.Style);

        return new TeamTactics
        {
            TeamName = (await team).Values.TeamName ?? string.Empty,
            Slots = await slots,
            Active = new ActiveTactic
            {
                InPossessionFormation = withBall.Name,
                OutOfPossessionFormation = withoutBall.Name,
                InPossessionFormationKey = Key(names.Formations, withBall.Id),
                OutOfPossessionFormationKey = Key(names.Formations, withoutBall.Id),
                Mentality = settings.Mentality,
                MentalityKey = Key(names.Mentalities, settings.MentalityId),
                Style = style?.DisplayValue,
                StyleKey = Key(names.Styles, style?.PropertyValue),
                TeamInstructions = await instructions,
                Lineup = await lineup
            },
            RecommendedStyles = RecommendedStyles(settings)
        };
    }

    public async Task<RoleFit> GetRoleFitAsync(PitchPosition position, bool inPossession, CancellationToken cancellationToken = default)
    {
        var squad = await bridge.GetListAsync<BridgeRoleFitPlayerDto>(
            BridgeQuery.FromHuman()
                .List(TacticConstants.SquadPath)
                .Context(TacticConstants.PositionMask, PositionMasks.Of(position))
                .Context(TacticConstants.InPossession, inPossession)
                .WithHumanAndTeam()
                .Props([GameConstants.UniqueId, GameConstants.Name, TacticConstants.RoleTable]),
            cancellationToken,
            gapsExpected: true);

        // One rating per player and role. The game is asked about a player by their index in the squad list.
        var roles = squad.Items
            .SelectMany((player, index) => (player.Values.Roles ?? []).Select(row => (Player: player.Values, Index: index, Row: row)))
            .Where(rating => rating.Row.TacticsRoleId is not null)
            .GroupBy(rating => rating.Row.TacticsRoleId!.Value)
            .ToList();

        var keyAttributes = await Task.WhenAll(
            roles.Select(role => KeyAttributesAsync(role.Key, inPossession, role.First().Index, cancellationToken)));
        var names = (await vocabulary.KeysAsync(cancellationToken)).Roles(inPossession);

        return new RoleFit
        {
            Position = position,
            InPossession = inPossession,
            Roles = roles.Select((role, i) => new RoleCandidates
            {
                Key = names.GetValueOrDefault(role.Key),
                Role = role.First().Row.TacticsRoleName ?? string.Empty,
                ShortName = role.First().Row.ShortName,
                KeyAttributes = keyAttributes[i],
                Players = role
                    .Select(rating => (rating.Player, Rating: rating.Row.CurrentAbilityStars.Adapt<Rating?>()))
                    .Where(rating => rating.Rating is not null)
                    .Select(rating => new PlayerRoleRating
                    {
                        PlayerId = rating.Player.Id ?? 0,
                        Name = rating.Player.Name ?? string.Empty,
                        Rating = rating.Rating!
                    })
                    .ToList()
            }).ToList()
        };
    }

    // The active tactic, with the user's manager and team as contexts like the game's own screens.
    private static BridgeQuery OnTactic()
    {
        return BridgeQuery.OnActiveTactic();
    }

    private async Task<BridgeFormationDto> FormationAsync(bool inPossession, CancellationToken cancellationToken)
    {
        var formation = await bridge.GetRecordAsync<BridgeFormationDto>(
            OnTactic()
                .Context(TacticConstants.InPossession, inPossession)
                .Props([TacticConstants.FormationName, TacticConstants.Formation]),
            cancellationToken);

        return formation.Values;
    }

    // Every team instruction by the keys tactic edits take. The game only has their catalog once its tactics screen has
    // been opened; until then, the main ones by the game's property names.
    private async Task<Dictionary<string, string?>> TeamInstructionsAsync(
        BridgeTacticOverviewDto settings, CancellationToken cancellationToken)
    {
        try
        {
            var instructions = await teamInstructionReader.ReadAsync(cancellationToken);
            return instructions.ToDictionary(instruction => instruction.Key, instruction => instruction.Current);
        }
        catch (GameUnavailableException) when (!cancellationToken.IsCancellationRequested)
        {
            return await PlainTeamInstructionsAsync(settings, cancellationToken);
        }
    }

    private async Task<Dictionary<string, string?>> PlainTeamInstructionsAsync(
        BridgeTacticOverviewDto settings, CancellationToken cancellationToken)
    {
        var chosen = settings.TeamInstructions
            .Where(instruction => instruction.Value.ValueKind == JsonValueKind.Number && instruction.Value.TryGetUInt64(out _))
            .ToDictionary(instruction => instruction.Key, instruction => instruction.Value.GetUInt64());

        var names = await bridge.GetNamesAsync(chosen.Values, cancellationToken);

        return chosen.ToDictionary(
            instruction => instruction.Key,
            instruction => names.GetValueOrDefault(instruction.Value.ToString(CultureInfo.InvariantCulture)));
    }

    private static List<string> RecommendedStyles(BridgeTacticOverviewDto settings)
    {
        var styles = (settings.StyleOptions ?? [])
            .Select(Unwrap)
            .Where(style => style.PropertyValue is not null && style.DisplayValue is not null)
            .ToDictionary(style => style.PropertyValue!.Value, style => style.DisplayValue!);

        return (settings.RecommendedStyles ?? []).Select(id => styles.GetValueOrDefault(id)).OfType<string>().ToList();
    }

    private async Task<List<LineupPosition>> LineupAsync(CancellationToken cancellationToken)
    {
        var keys = vocabulary.KeysAsync(cancellationToken);
        var inPossession = LineupPhaseAsync(inPossession: true, cancellationToken);
        var outOfPossession = LineupPhaseAsync(inPossession: false, cancellationToken);

        await Task.WhenAll(keys, inPossession, outOfPossession);

        // Both answer in lineup order, one row per slot index.
        var names = await keys;
        var withBall = (await inPossession).Items;
        var withoutBall = (await outOfPossession).Items;

        var positions = Enumerable.Range(0, Math.Min(withBall.Count, withoutBall.Count)).Select(async index =>
        {
            var inPossessionAssignment = AssignmentAsync(index, true, withBall[index].Values, names, cancellationToken);
            var outOfPossessionAssignment = AssignmentAsync(index, false, withoutBall[index].Values, names, cancellationToken);

            return new LineupPosition
            {
                Index = index,
                PlayerId = withBall[index].Values.PlayerId,
                PlayerName = withBall[index].Values.PlayerName,
                InPossession = await inPossessionAssignment,
                OutOfPossession = await outOfPossessionAssignment
            };
        });

        return (await Task.WhenAll(positions)).ToList();
    }

    private Task<BridgeListDto<BridgeLineupSlotDto>> LineupPhaseAsync(bool inPossession, CancellationToken cancellationToken)
    {
        return bridge.GetListAsync<BridgeLineupSlotDto>(
            OnTactic()
                .Context(TacticConstants.InPossession, inPossession)
                .Each(TacticConstants.TeamSelectionIndex, 0, LineupSize - 1)
                .Props([
                    TacticConstants.LineupPlayerId, TacticConstants.LineupPlayerName, TacticConstants.PositionValue,
                    TacticConstants.RoleValue, TacticConstants.LineupRoleRating
                ]),
            cancellationToken);
    }

    // The slot's role is only a mask; its name comes from the player's role table at the slot's position, which also
    // rates them in every other role available there.
    private async Task<PositionAssignment> AssignmentAsync(
        int index, bool inPossession, BridgeLineupSlotDto slot, TacticKeys names, CancellationToken cancellationToken)
    {
        List<BridgeRoleRowDto> roles = [];

        if (slot.PlayerId is not null && slot.Position is { } position)
        {
            var table = await bridge.GetRecordAsync<BridgeLineupRoleTableDto>(
                OnTactic()
                    .Context(TacticConstants.TeamSelectionIndex, index)
                    .Context(TacticConstants.PositionMask, PositionMasks.WithoutSide(position))
                    .Context(TacticConstants.InPossession, inPossession)
                    .Props([TacticConstants.LineupRoleTable]),
                cancellationToken);

            roles = table.Values.Roles ?? [];
        }

        var current = roles.FirstOrDefault(role => role.TacticsRoleId == slot.Role);
        var keys = names.Roles(inPossession);

        return new PositionAssignment
        {
            Position = slot.Position is { } mask ? PositionMasks.ToPosition(mask) : null,
            RoleKey = slot.Role is { } role ? keys.GetValueOrDefault(role) : null,
            Role = current?.TacticsRoleName,
            RoleShortName = current?.ShortName,
            Rating = slot.Rating.Adapt<Rating?>(),
            RoleOptions = roles.BuildAdapter().AddParameters(MappingConfig.RoleKeysParameter, keys).AdaptToType<List<RoleRating>>()
        };
    }

    private async Task<List<TacticSlot>> SlotsAsync(BridgeTacticOverviewDto settings, CancellationToken cancellationToken)
    {
        string?[] names = [settings.Slot1Name, settings.Slot2Name, settings.Slot3Name];
        bool?[] valid = [settings.Slot1Valid, settings.Slot2Valid, settings.Slot3Valid];
        var indexes = Enumerable.Range(0, SlotCount).Where(index => valid[index] == true).ToList();

        var effectiveness = bridge.GetListAsync<BridgeSlotEffectivenessDto>(
            OnTactic().Each(TacticConstants.TacticIndex, 0, SlotCount - 1).Props(TacticConstants.SlotEffectivenessParts),
            cancellationToken,
            gapsExpected: true);

        // The game lists a slot's positions for a given player; any squad player will do.
        var positions = bridge.GetListAsync<BridgeSlotPositionsDto>(
            BridgeQuery.FromHuman()
                .List(TacticConstants.SquadPath)
                .Index(0)
                .Ref(TacticConstants.HumanTeam, TacticConstants.TeamPath)
                .Each(TacticConstants.TacticIndex, 0, SlotCount - 1)
                .WithHumanAndTeam()
                .Props([TacticConstants.SlotPositions]),
            cancellationToken);

        var players = indexes.ToDictionary(index => index, index => SlotPlayersAsync(index, cancellationToken));

        await Task.WhenAll([effectiveness, positions, .. players.Values]);

        return indexes.Select(index => new TacticSlot
        {
            Slot = index + 1,
            Name = names[index] ?? string.Empty,
            IsActive = settings.CurrentSlot == index,
            Effectiveness = effectiveness.Result.Items[index].Values.Adapt<TacticalEffectiveness>(),
            Lineup = SlotLineup(positions.Result.Items[index].Values, players[index].Result)
        }).ToList();
    }

    private Task<BridgeListDto<BridgeSlotPlayerDto>> SlotPlayersAsync(int slot, CancellationToken cancellationToken)
    {
        return bridge.GetListAsync<BridgeSlotPlayerDto>(
            BridgeQuery.FromHuman()
                .List(TacticConstants.SlotPlayers)
                .Context(TacticConstants.TacticFlag, true)
                .Context(TacticConstants.TacticIndex, slot)
                .WithHumanAndTeam()
                .Props([TacticConstants.SlotPlayerId, TacticConstants.SlotPlayerName, TacticConstants.SlotPlayerEffectiveness]),
            cancellationToken);
    }

    // The players come in lineup order; the labels are keyed by slot index, the player's own slot listed twice.
    private static List<SlotPosition> SlotLineup(BridgeSlotPositionsDto positions, BridgeListDto<BridgeSlotPlayerDto> players)
    {
        var labels = (positions.Positions ?? [])
            .Select(Unwrap)
            .Where(position => position.PropertyValue is not null)
            .GroupBy(position => (int)position.PropertyValue!.Value)
            .ToDictionary(group => group.Key, group => group.First().DisplayValue);

        return Enumerable.Range(0, LineupSize).Select(index =>
        {
            var position = index < players.Items.Count ? players.Items[index].Values.Adapt<SlotPosition>() : new SlotPosition();
            position.Index = index;
            position.InPossession = labels.GetValueOrDefault(index);
            return position;
        }).ToList();
    }

    // The game's tactic lookups for one role list its key attributes with a player's values; any player will do.
    private async Task<List<string>> KeyAttributesAsync(
        ulong role, bool inPossession, int playerIndex, CancellationToken cancellationToken)
    {
        var answer = await bridge.GetListAsync<BridgeKeyAttributesDto>(
            OnTactic()
                .List(TacticConstants.SquadPath)
                .Index(playerIndex)
                .ItemAs(TacticConstants.Person)
                .Context(TacticConstants.RoleMask, role)
                .Context(TacticConstants.InPossession, inPossession)
                .Props([TacticConstants.KeyAttributesForRole]),
            cancellationToken);

        return answer.Items.FirstOrDefault()?.Values.Attributes?
            .Select(attribute => attribute.DisplayValue)
            .OfType<string>()
            .ToList() ?? [];
    }

    private static BridgeOptionDto Unwrap(BridgeOptionDto option)
    {
        return option.RowData ?? option;
    }

    private static string? Key(IReadOnlyDictionary<long, string> names, long? id)
    {
        return id is { } value ? names.GetValueOrDefault(value) : null;
    }
}
