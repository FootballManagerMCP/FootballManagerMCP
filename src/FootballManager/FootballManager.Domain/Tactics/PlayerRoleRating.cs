namespace FootballManager.Domain.Tactics;

/// <summary>A squad player's rating in a role, 0-20.</summary>
public class PlayerRoleRating
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }
    public required Rating Rating { get; set; }
}
