namespace FootballManager.Domain.Scouting;

/// <summary>One entry of a game menu: its label in the game's language, and either sub-entries or something it does.</summary>
public class MenuEntry
{
    public required string Label { get; set; }

    /// <summary>The entry does something when picked (it isn't only a sub-menu, or greyed out).</summary>
    public bool CanRun { get; set; }

    /// <summary>What the game asks the user to confirm before running it, in its display language, when it asks.</summary>
    public string? Confirmation { get; set; }

    /// <summary>The game shows that confirmation as an error: a warning against going ahead.</summary>
    public bool ConfirmationIsWarning { get; set; }

    public List<MenuEntry> Children { get; set; } = [];
}
