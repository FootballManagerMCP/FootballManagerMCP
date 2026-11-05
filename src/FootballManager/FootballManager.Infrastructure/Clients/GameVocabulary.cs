using System.Text.Json;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;
using FootballManager.Infrastructure.Mappings;

namespace FootballManager.Infrastructure.Clients;

// What the game calls the things a tactic holds as numbers, read once per request: its enums for styles, mentalities,
// formations and roles, and the tactics screen's catalogs of team and player instructions.
internal sealed class GameVocabulary(GameBridge bridge)
{
    private Task<TacticKeys>? _keys;
    private Task<List<BridgeTeamInstructionDto>>? _teamInstructions;
    private Task<List<BridgePlayerInstructionGroupDto>>? _playerInstructions;

    public Task<TacticKeys> KeysAsync(CancellationToken cancellationToken)
    {
        return _keys ??= LoadKeysAsync(cancellationToken);
    }

    /// <summary>The game only has these once its tactics screen has been opened.</summary>
    public Task<List<BridgeTeamInstructionDto>> TeamInstructionsAsync(CancellationToken cancellationToken)
    {
        return _teamInstructions ??= bridge.GetJsonAsync<List<BridgeTeamInstructionDto>>("/instructions", cancellationToken);
    }

    public Task<List<BridgePlayerInstructionGroupDto>> PlayerInstructionsAsync(CancellationToken cancellationToken)
    {
        return _playerInstructions ??= bridge.GetJsonAsync<List<BridgePlayerInstructionGroupDto>>("/player-instructions", cancellationToken);
    }

    private async Task<TacticKeys> LoadKeysAsync(CancellationToken cancellationToken)
    {
        var enums = await bridge.GetJsonAsync<Dictionary<string, Dictionary<string, JsonElement>>>(
            $"/enums?types={Uri.EscapeDataString(string.Join(',', TacticConstants.Enums))}", cancellationToken);

        return TacticKeys.From(enums);
    }
}
