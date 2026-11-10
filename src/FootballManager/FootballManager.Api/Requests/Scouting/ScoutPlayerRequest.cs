using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Requests.Scouting;

public class ScoutPlayerRequest
{
    /// <summary>Who watches him, by name as the game writes it; any scout when left out.</summary>
    public string? Scout { get; set; }

    public ScoutingDuration Duration { get; set; } = ScoutingDuration.UntilFullKnowledge;
}
