namespace FootballManager.Api.Responses.Training;

public class TrainingSessionResponse
{
    public int Slot { get; set; }
    public int Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Category { get; set; }

    /// <summary>True when the user can change it; left out otherwise.</summary>
    public bool? CanEdit { get; set; }
}
