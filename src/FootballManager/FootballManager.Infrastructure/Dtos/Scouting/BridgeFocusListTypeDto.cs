using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>Which of its focus lists the scouting screen shows.</summary>
internal sealed class BridgeFocusListTypeDto
{
    [JsonPropertyName(ScoutingConstants.FocusListType)]
    public int? ListType { get; set; }
}
