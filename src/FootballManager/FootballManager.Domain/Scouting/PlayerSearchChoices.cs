using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Scouting;

/// <summary>What the player search's rules can take, as the game offers it.</summary>
public class PlayerSearchChoices
{
    public List<int> Ages { get; set; } = [];
    public List<PitchPosition> Positions { get; set; } = [];
    public List<PositionFamiliarity> Familiarities { get; set; } = [];

    /// <summary>The scouts' grades, best first ("A+" to "F").</summary>
    public List<string> Grades { get; set; } = [];
}
