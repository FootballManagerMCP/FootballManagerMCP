using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Competitions;

/// <summary>A player of a competition's stats screen, cross-checked with the user's scouting and his contract.</summary>
public class CompetitionPlayer
{
    /// <summary>The player as the user's scouting knows him: stars, grade, value and wage ranges, statuses.</summary>
    public required ScoutedPlayer Player { get; set; }

    public int? TeamId { get; set; }

    /// <summary>Every position he can play.</summary>
    public List<PitchPosition> Positions { get; set; } = [];

    /// <summary>His club season so far, all competitive matches together.</summary>
    public double? Minutes { get; set; }

    /// <summary>His value and rank in each stat read; a stat he isn't in the game's top 100 for is left out.</summary>
    public List<CompetitionStatValue> Stats { get; set; } = [];

    public PlayerContract? Contract { get; set; }
}
