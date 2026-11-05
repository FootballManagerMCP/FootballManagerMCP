using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>The option a team instruction or a player instruction group holds, as the catalogs' option ids.</summary>
internal sealed class BridgeInstructionValueDto
{
    [JsonPropertyName(TacticConstants.SelectedTeamInstruction)]
    public ulong? TeamInstruction { get; set; }

    [JsonPropertyName(TacticConstants.SelectedPlayerInstruction)]
    public ulong? PlayerInstruction { get; set; }

    [JsonPropertyName(TacticConstants.PlayerInstructionLocked)]
    public bool? PlayerInstructionLocked { get; set; }
}
