namespace FootballManager.Domain.Scouting;

/// <summary>A scouting change to one player, made through his menu in the game, or planned by a dry run.</summary>
public class PlayerScoutingChange
{
    /// <summary>False for a dry run, which only checks.</summary>
    public bool Applied { get; set; }

    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public PlayerMenuAction Action { get; set; }

    /// <summary>The menu entry picked, as the game labels it: "Assign scout > David Pleat (Scout) > 1 week".</summary>
    public required string Choice { get; set; }

    /// <summary>What the game asks the user to confirm for this entry (for a scout: the costs to expect), when it asks.</summary>
    public string? Confirmation { get; set; }

    /// <summary>The game words that confirmation as a warning against going ahead; such a change isn't applied.</summary>
    public bool ConfirmationIsWarning { get; set; }

    /// <summary>For a scout sent: the scouting budget and spending before it.</summary>
    public ScoutingBudget? Budget { get; set; }

    /// <summary>As the game reads back after the change; for a dry run, as it is now.</summary>
    public bool? IsShortlisted { get; set; }
    public bool? IsBeingScouted { get; set; }

    /// <summary>For a trial offered or ended: where he stands with it after the change; for a dry run, now.</summary>
    public TrialState? Trial { get; set; }
}
