using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;

namespace FootballManager.Domain.Competitions;

/// <summary>A player of a stat's table read in full: the user's scouting knowledge of him, and his contract.</summary>
public class CompetitionPlayerProfile
{
    public required ScoutedPlayer Player { get; set; }
    public PlayerContract? Contract { get; set; }
}
