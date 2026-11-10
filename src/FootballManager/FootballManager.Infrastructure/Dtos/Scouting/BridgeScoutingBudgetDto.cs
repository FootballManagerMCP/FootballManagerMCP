using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The scouting budget and costs, read from the user's manager.</summary>
internal sealed class BridgeScoutingBudgetDto
{
    [JsonPropertyName(ScoutingConstants.Budget)]
    public long? Budget { get; set; }

    [JsonPropertyName(ScoutingConstants.CostsThisSeason)]
    public long? SpentThisSeason { get; set; }

    [JsonPropertyName(ScoutingConstants.CostsThisMonth)]
    public long? SpentThisMonth { get; set; }
}
