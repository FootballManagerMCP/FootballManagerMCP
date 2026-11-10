using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The player search's own settings: its interest, and how many conditions its filter holds.</summary>
internal sealed class BridgeSearchStateDto
{
    [JsonPropertyName(PlayerSearchConstants.InterestSelection)]
    public BridgeInterestOptionDto? Interest { get; set; }

    [JsonPropertyName(PlayerSearchConstants.EditFilterLabel)]
    public string? EditFilterLabel { get; set; }
}
