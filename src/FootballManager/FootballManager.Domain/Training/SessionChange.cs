namespace FootballManager.Domain.Training;

/// <summary>A schedule session's planned or applied change.</summary>
public class SessionChange
{
    public string Date { get; set; } = string.Empty;
    public int Slot { get; set; }
    public OptionChange Change { get; set; } = new();
}
