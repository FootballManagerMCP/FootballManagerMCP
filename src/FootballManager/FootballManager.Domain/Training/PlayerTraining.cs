namespace FootballManager.Domain.Training;

/// <summary>A player's individual training, as the game's individual training screen shows it.</summary>
public class PlayerTraining
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>The position he trains in, an FM code ("MR"). He trains in the one he plays unless set otherwise.</summary>
    public string? Position { get; set; }

    /// <summary>The in-possession role he trains for: the game's key ("Winger") and its label.</summary>
    public string? RoleKey { get; set; }

    public string? RoleName { get; set; }

    /// <summary>Whether the position and role follow what he plays in matches (the game's default).</summary>
    public bool FollowsMatchRole { get; set; }

    /// <summary>The out-of-possession role he trains for, when one is set.</summary>
    public string? OutOfPossessionRole { get; set; }

    /// <summary>His additional focus, when he has one.</summary>
    public TrainingOption? Focus { get; set; }

    /// <summary>His training intensity, or the rest he's been given.</summary>
    public TrainingOption? Intensity { get; set; }

    /// <summary>Whether the intensity is the game's automatic choice (Intensity then says what it picked).</summary>
    public bool IntensityIsAutomatic { get; set; }

    /// <summary>The days of rest he has left, when he's been rested.</summary>
    public int? RestDays { get; set; }

    /// <summary>His average training rating (0-10), when he's trained lately.</summary>
    public double? Rating { get; set; }

    /// <summary>His workload, as the game words it.</summary>
    public string? Workload { get; set; }

    /// <summary>Whether the user sets his additional focus (false while the staff handle his training).</summary>
    public bool CanEditFocus { get; set; }

    /// <summary>Whether the user sets his intensity (false while he's injured: the medical staff do).</summary>
    public bool CanEditIntensity { get; set; }

    /// <summary>The ids of the additional focuses his own dropdown offers (keepers, outfield and injured players differ).</summary>
    public List<long> FocusChoices { get; set; } = [];

    /// <summary>The ids of the intensities and rest periods his own dropdown offers.</summary>
    public List<long> IntensityChoices { get; set; } = [];
}
