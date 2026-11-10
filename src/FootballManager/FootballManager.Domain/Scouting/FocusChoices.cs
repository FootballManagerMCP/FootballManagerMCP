using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Scouting;

/// <summary>What the focus wizard lets a new focus take: the ages its range can start and end at, and its positions.</summary>
public class FocusChoices
{
    public List<int> MinAges { get; set; } = [];
    public List<int> MaxAges { get; set; } = [];
    public List<TacticalPosition> Positions { get; set; } = [];
}
