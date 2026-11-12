using FootballManager.Domain.Tactics;

namespace FootballManager.Domain.Competitions;

/// <summary>Which players of a competition's stats screen to rank, and how to narrow them down.</summary>
public class CompetitionStatsQuery
{
    /// <summary>The competition's name (in the game's language) or FM id; the user's league when left out.</summary>
    public string? Competition { get; set; }

    public List<CompetitionStat> Stats { get; set; } = [];

    /// <summary>The stat players are picked and ranked by; the first of <see cref="Stats"/> when left out.</summary>
    public CompetitionStat? SortBy { get; set; }

    /// <summary>Players who can play any of these; every position when empty.</summary>
    public List<PitchPosition> Positions { get; set; } = [];

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    /// <summary>Minutes for his club this season, all competitive matches together.</summary>
    public int? MinMinutes { get; set; }

    /// <summary>The user's own players are left out unless asked for.</summary>
    public bool IncludeOwnPlayers { get; set; }

    public int Limit { get; set; } = 25;
}
