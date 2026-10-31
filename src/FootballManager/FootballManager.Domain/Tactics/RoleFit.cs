namespace FootballManager.Domain.Tactics;

/// <summary>Who in the squad suits each role available at one position, in or out of possession.</summary>
public class RoleFit
{
    public required PitchPosition Position { get; set; }
    public bool InPossession { get; set; }
    public List<RoleCandidates> Roles { get; set; } = [];
}
