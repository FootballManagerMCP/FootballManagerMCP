using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Entities;

/// <summary>A role a player could fill at a position, and how well the game rates them in it.</summary>
public class RoleRating
{
    public required string Role { get; set; }
    public string? ShortName { get; set; }
    public Rating? Rating { get; set; }
}
