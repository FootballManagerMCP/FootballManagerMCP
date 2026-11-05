namespace FootballManager.Domain.Tactics.Editing;

/// <summary>One write of a tactic edit, e.g. a team instruction's new option or a player's move.</summary>
public class TacticEditStep
{
    /// <summary>What it changes: "Switch slot", "Style", "Mentality", "Team instruction", "Formation", "Pick", "Move", "Role", "Player instruction".</summary>
    public required string Action { get; set; }

    /// <summary>Which instruction, phase or player it applies to.</summary>
    public string? Subject { get; set; }

    public string? From { get; set; }
    public string? To { get; set; }
    public EditStepStatus Status { get; set; }

    /// <summary>Why it failed, or what else to know.</summary>
    public string? Detail { get; set; }
}
