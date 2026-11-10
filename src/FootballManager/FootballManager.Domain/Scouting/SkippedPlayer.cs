namespace FootballManager.Domain.Scouting;

/// <summary>A player a team-wide change left out, and why.</summary>
public class SkippedPlayer
{
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public required string Reason { get; set; }
}
