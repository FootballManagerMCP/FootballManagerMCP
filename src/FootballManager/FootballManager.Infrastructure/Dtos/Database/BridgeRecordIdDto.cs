using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Database;

/// <summary>A record's id, to check a path still leads to it.</summary>
internal sealed class BridgeRecordIdDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }
}
