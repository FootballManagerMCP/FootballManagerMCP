using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Scouting;

namespace FootballManager.Infrastructure.Dtos.Competitions;

/// <summary>A player's contract with his club and what he'd cost, keyed by the game's property paths.</summary>
internal sealed class BridgePlayerContractDto
{
    /// <summary>yyyy-MM-dd; the game dates a contract without an end 1900.</summary>
    [JsonPropertyName(CompetitionConstants.ContractEndDate)]
    public string? EndDate { get; set; }

    [JsonPropertyName(CompetitionConstants.ContractHasEndDate)]
    public bool? HasEndDate { get; set; }

    [JsonPropertyName(CompetitionConstants.ContractType)]
    public BridgeSortedValueDto? Type { get; set; }

    [JsonPropertyName(CompetitionConstants.ContractStatus)]
    public BridgeSortedValueDto? Status { get; set; }

    /// <summary>The weekly wage as a number and as written ("N/A" with 0 when it doesn't apply).</summary>
    [JsonPropertyName(CompetitionConstants.Wage)]
    public BridgeOptionDto? Wage { get; set; }

    [JsonPropertyName(CompetitionConstants.TransferValue)]
    public BridgeTransferValueDto? Value { get; set; }

    [JsonPropertyName(CompetitionConstants.TransferStatus)]
    public BridgeTransferStatusDto? TransferStatus { get; set; }
}
