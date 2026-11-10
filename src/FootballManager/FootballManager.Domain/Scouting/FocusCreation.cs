namespace FootballManager.Domain.Scouting;

/// <summary>A recruitment focus created, or planned by a dry run.</summary>
public class FocusCreation
{
    /// <summary>False for a dry run, which only checks.</summary>
    public bool Applied { get; set; }

    public required FocusDraft Draft { get; set; }

    /// <summary>The focus as the game lists it once created; null for a dry run, or while the game's list lags.</summary>
    public RecruitmentFocus? Focus { get; set; }

    /// <summary>What the reader should know, e.g. the game's list not showing the new focus yet.</summary>
    public List<string> Notes { get; set; } = [];
}
