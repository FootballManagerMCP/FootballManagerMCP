namespace FootballManager.Domain.Scouting;

/// <summary>A recruitment focus removed, or planned for removal by a dry run.</summary>
public class FocusRemoval
{
    /// <summary>False for a dry run, which only checks.</summary>
    public bool Applied { get; set; }

    public required RecruitmentFocus Focus { get; set; }
}
