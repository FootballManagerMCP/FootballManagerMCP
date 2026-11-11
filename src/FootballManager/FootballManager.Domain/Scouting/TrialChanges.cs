namespace FootballManager.Domain.Scouting;

/// <summary>Trials offered or ended through the players' menus in the game, or planned by a dry run.</summary>
public class TrialChanges
{
    /// <summary>False for a dry run, which only checks.</summary>
    public bool Applied { get; set; }

    /// <summary>For trials offered: how long each lasts, in weeks.</summary>
    public int? Weeks { get; set; }

    /// <summary>The players changed (or who would be), each with the menu entry picked for him.</summary>
    public List<PlayerScoutingChange> Players { get; set; } = [];

    /// <summary>Players left out: the game doesn't offer the change for them now, or warns against it.</summary>
    public List<SkippedPlayer> Skipped { get; set; } = [];

    /// <summary>Ids found in none of the lists the tools can reach a player through.</summary>
    public List<int> NotFound { get; set; } = [];

    /// <summary>What stopped the run before the end, or what the game didn't show after a change.</summary>
    public List<string> Notes { get; set; } = [];
}
