namespace FootballManager.Domain.Matches;

/// <summary>One kick of a penalty shoot-out.</summary>
public class PenaltyKick
{
    public bool IsOurs { get; set; }
    public string? Taker { get; set; }
    public bool Scored { get; set; }
}
