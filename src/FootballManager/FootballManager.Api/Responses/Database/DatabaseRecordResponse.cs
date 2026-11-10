using FootballManager.Domain.Database;

namespace FootballManager.Api.Responses.Database;

/// <summary>A result of the game's search. Only what the game shows for its kind is sent.</summary>
public class DatabaseRecordResponse
{
    public DatabaseRecordKind Kind { get; set; }
    public int? Id { get; set; }
    public required string Name { get; set; }
    public string? LongName { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public int? Age { get; set; }
    public string? Positions { get; set; }
    public string? Job { get; set; }
    public string? Nation { get; set; }
    public string? Club { get; set; }
    public int? ClubId { get; set; }
    public string? ClubNation { get; set; }
    public string? League { get; set; }
    public string? Continent { get; set; }
}
