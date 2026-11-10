using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Requests.Scouting;

public class ScoutTeamRequest
{
    /// <summary>Who watches them, by name as the game writes it; any scout when left out.</summary>
    public string? Scout { get; set; }

    public ScoutingDuration Duration { get; set; } = ScoutingDuration.UntilFullKnowledge;

    /// <summary>Needed to apply: how many players the dry run listed, as the user agreed to it.</summary>
    public int? ExpectedPlayers { get; set; }
}
