using FootballManager.Domain.Matches;

namespace FootballManager.Api.Responses.Matches;

public class MatchGoalResponse
{
    public required string Minute { get; set; }
    public bool IsOurs { get; set; }

    /// <summary>Only sent for a penalty or an own goal.</summary>
    public GoalKind? Kind { get; set; }

    public string? Scorer { get; set; }
    public string? Assister { get; set; }
}
