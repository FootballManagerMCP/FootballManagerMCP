namespace FootballManager.Domain.Tactics.Editing;

/// <summary>
/// A group of player instructions of which a player takes at most one (e.g. Shooting: ShootMore or ShootLess), for one
/// phase. Which ones a player can take depends on their position and role.
/// </summary>
public class PlayerInstructionGroup
{
    public required string Key { get; set; }
    public string? Label { get; set; }
    public bool InPossession { get; set; }
    public List<TacticOption> Options { get; set; } = [];
}
