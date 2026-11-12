namespace FootballManager.Api.Requests.Training;

public class SessionRequest
{
    /// <summary>The day, yyyy-MM-dd, in this week or the next.</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>The session's place in the day, 0 to 2.</summary>
    public int Slot { get; set; }

    /// <summary>The new session's id from get_training's sessionTypes.</summary>
    public int Session { get; set; }
}
