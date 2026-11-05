using System.Globalization;
using FootballManager.Domain.Tactics.Editing;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// The active tactic's team instructions by key, each with the options the tactics screen offers and the one the
// tactic holds. The tactic holds an option as its property id, read per instruction id.
internal sealed class TeamInstructionReader(GameBridge bridge, GameVocabulary vocabulary)
{
    public async Task<List<TeamInstructionOptions>> ReadAsync(CancellationToken cancellationToken)
    {
        var catalog = vocabulary.TeamInstructionsAsync(cancellationToken);
        var values = bridge.GetListAsync<BridgeInstructionValueDto>(
            BridgeQuery.OnActiveTactic()
                .Each(TacticConstants.TeamInstructionId, 0, TacticConstants.TeamInstructionIdCount - 1)
                .Props([TacticConstants.SelectedTeamInstruction]),
            cancellationToken,
            gapsExpected: true);

        await Task.WhenAll(catalog, values);

        // The rows come in instruction id order.
        var held = (await values).Items;
        var entries = (await catalog).Where(entry => entry.Instruction is not null && entry.TeamInstructionId is not null).ToList();
        var chosen = entries.ToDictionary(entry => entry, entry => held.ElementAtOrDefault(entry.TeamInstructionId!.Value)?.Values.TeamInstruction);

        // A tactic can hold an option the screen lists under another name (a style's reset sets CrossingStyleNone where
        // the screen offers CrossingNone): those go by the game's property name.
        var unlisted = chosen.Values.OfType<ulong>().Where(id => !entries.Any(entry => entry.Options.Any(option => option.Id == id))).ToList();
        var names = await bridge.GetNamesAsync(unlisted, cancellationToken);

        return entries.Select(entry =>
        {
            var instruction = entry.Adapt<TeamInstructionOptions>();
            var id = chosen[entry];
            instruction.Current = OptionKey(entry, id) ?? (id is { } value ? names.GetValueOrDefault(Format(value)) ?? Format(value) : null);
            return instruction;
        }).ToList();
    }

    /// <summary>The key of the catalog's option with <paramref name="id"/>, or null.</summary>
    public static string? OptionKey(BridgeTeamInstructionDto entry, ulong? id)
    {
        return id is null ? null : entry.Options.FirstOrDefault(option => option.Id == id)?.Option;
    }

    private static string Format(ulong value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
