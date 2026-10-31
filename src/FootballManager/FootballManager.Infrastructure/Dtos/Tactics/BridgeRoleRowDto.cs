namespace FootballManager.Infrastructure.Dtos.Tactics;

/// <summary>A row of a player's role table at a position: a role and the player's star rating in it.</summary>
internal sealed class BridgeRoleRowDto
{
    /// <summary>The role's mask, as lineup slots hold it.</summary>
    public ulong? TacticsRoleId { get; set; }
    public string? TacticsRoleName { get; set; }
    public string? ShortName { get; set; }
    public BridgeRatingDto? CurrentAbilityStars { get; set; }
}
