using FootballManager.Domain.Competitions;
using FootballManager.Domain.Tactics;

namespace FootballManager.Api.Requests.Competitions;

public class CompetitionStatsRequest
{
    /// <summary>The competition's name (in the game's language) or FM id, as get_competitions lists it; the user's league when left out.</summary>
    public string? Competition { get; set; }

    /// <summary>The stats to read, at most 10.</summary>
    public List<CompetitionStat> Stats { get; set; } = [];

    /// <summary>The stat players are picked and ranked by; the first stat when left out.</summary>
    public CompetitionStat? SortBy { get; set; }

    /// <summary>Only players who can play one of these positions.</summary>
    public List<PitchPosition> Positions { get; set; } = [];

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    /// <summary>Minutes for his club this season, all competitive matches together.</summary>
    public int? MinMinutes { get; set; }

    public bool IncludeOwnPlayers { get; set; }

    /// <summary>1 to 50.</summary>
    public int Limit { get; set; } = 25;
}
