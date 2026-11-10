using System.Text.Json.Serialization;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>One of the player search's standing rules, as its filter editor shows it.</summary>
internal sealed class BridgeSearchRuleDto
{
    // Which rule it is, set by the client: the game doesn't send it.
    [JsonIgnore]
    public SearchRuleKind Kind { get; set; }

    public bool? Enabled { get; set; }
    public string? Name { get; set; }

    [JsonPropertyName(PlayerSearchConstants.Comparison)]
    public BridgeCompareFlagDto? Comparison { get; set; }

    [JsonPropertyName(PlayerSearchConstants.Value)]
    public BridgeFilterItemDto? Value { get; set; }

    public BridgeFilterItemDto? MinValue { get; set; }
    public BridgeFilterItemDto? MaxValue { get; set; }

    [JsonPropertyName(PlayerSearchConstants.ValueText)]
    public string? ValueText { get; set; }

    // What it can take: its values (Items), the minimums, and its comparisons.
    [JsonPropertyName(PlayerSearchConstants.Values)]
    public List<BridgeFilterItemDto>? Values { get; set; }

    [JsonPropertyName(PlayerSearchConstants.MinValues)]
    public List<BridgeFilterItemDto>? MinValues { get; set; }

    [JsonPropertyName(PlayerSearchConstants.Comparisons)]
    public List<BridgeCompareFlagDto>? Comparisons { get; set; }
}
