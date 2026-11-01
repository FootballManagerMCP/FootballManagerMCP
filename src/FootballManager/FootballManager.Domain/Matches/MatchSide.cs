namespace FootballManager.Domain.Matches;

/// <summary>One team's side of a fixture.</summary>
public class MatchSide
{
    public int TeamId { get; set; }
    public required string TeamName { get; set; }

    /// <summary>Null until the match is played.</summary>
    public int? Goals { get; set; }
    public double? ExpectedGoals { get; set; }

    /// <summary>Share of the ball, in percent.</summary>
    public int? Possession { get; set; }
    public int? Shots { get; set; }
    public int? ShotsOnTarget { get; set; }

    /// <summary>Their current league position as the game writes it (e.g. "3e"), for league matches.</summary>
    public string? LeaguePosition { get; set; }
}
