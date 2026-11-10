using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Database;

/// <summary>A result of the game's search, with the record it stands for.</summary>
internal sealed class BridgeDatabaseRecordDto
{
    [JsonPropertyName(GameConstants.Name)]
    public string? Name { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.RecordType)]
    public int? RecordType { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.TypeLabel)]
    public string? TypeLabel { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.Description)]
    public string? Description { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.Linked)]
    public string? Linked { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.SecondaryLinked)]
    public string? SecondaryLinked { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.Id)]
    public int? Id { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.LongName)]
    public string? LongName { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.IsPlayer)]
    public bool? IsPlayer { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.Age)]
    public int? Age { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.Positions)]
    public string? Positions { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.Job)]
    public string? Job { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.Nation)]
    public string? Nation { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.Club)]
    public string? Club { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.ClubId)]
    public int? ClubId { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.ClubNation)]
    public string? ClubNation { get; set; }

    [JsonPropertyName(DatabaseSearchConstants.League)]
    public string? League { get; set; }
}
