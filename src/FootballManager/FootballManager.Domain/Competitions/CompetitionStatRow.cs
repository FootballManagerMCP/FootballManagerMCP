using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Competitions;

/// <summary>A player's row in a stat's table, with what's needed to filter him before reading him in full.</summary>
public class CompetitionStatRow
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }
    public int? Age { get; set; }

    /// <summary>Every position he can play.</summary>
    public List<PitchPosition> Positions { get; set; } = [];

    public int? TeamId { get; set; }
    public string? Team { get; set; }

    /// <summary>His club season so far, all competitive matches together.</summary>
    public double? Minutes { get; set; }

    public double Value { get; set; }

    /// <summary>His place in the table, 1 for the best; tied players share one.</summary>
    public int Rank { get; set; }

    /// <summary>His row's place in the game's list, which reaches him for the cross-check.</summary>
    public int RowIndex { get; set; }
}
