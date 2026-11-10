using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Scouting;

public class PlayerScoutingChangeResponse
{
    public bool Applied { get; set; }
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public PlayerMenuAction Action { get; set; }

    /// <summary>The game's menu entry, level by level, in its display language.</summary>
    public required string Choice { get; set; }

    /// <summary>What the game asks the user to confirm for this entry (for a scout: the costs to expect).</summary>
    public string? Confirmation { get; set; }

    /// <summary>Only sent when true: the game words the confirmation as a warning, and the change isn't applied.</summary>
    public bool? ConfirmationIsWarning { get; set; }

    public ScoutingBudgetResponse? Budget { get; set; }

    public bool? IsShortlisted { get; set; }
    public bool? IsBeingScouted { get; set; }
}
