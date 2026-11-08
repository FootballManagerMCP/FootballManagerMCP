using System.Globalization;
using System.Text;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Teams;

namespace FootballManager.Application.Services;

public interface ITeamsService
{
    /// <summary>A scouting report on a team, named in the game's language (e.g. "Barcelone") or by its FM id.</summary>
    Task<Result<TeamReport>> GetTeamAsync(string team, CancellationToken cancellationToken = default);
}

// Builds a scouting report from a team's competitive matches: its record, its numbers per match against what its
// opponents managed (overall, home and away), the shapes and starters it uses, and its squad.
public class TeamsService(ITeamsGameClient gameClient) : ITeamsService
{
    public const int RegularPlayersShown = 16;
    private const int SuggestionsShown = 12;

    public async Task<Result<TeamReport>> GetTeamAsync(string team, CancellationToken cancellationToken = default)
    {
        try
        {
            var found = Find(team, await gameClient.GetTeamsAsync(cancellationToken));

            if (found.IsFailed)
            {
                return found.ToResult<TeamReport>();
            }

            return Result.Ok(Report(await gameClient.GetSeasonAsync(found.Value.Id, cancellationToken)));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TeamReport>(new GameUnavailableError(exception.Message));
        }
    }

    // By FM id, by name (ignoring case and accents), or by a name that contains or is contained in the one given.
    private static Result<KnownTeam> Find(string team, List<KnownTeam> teams)
    {
        var known = teams.DistinctBy(t => t.Id).ToList();

        if (int.TryParse(team, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            return known.FirstOrDefault(t => t.Id == id) is { } byId
                ? Result.Ok(byId)
                : Fail($"No team with id {id} in the user's league or competitions.");
        }

        var wanted = Normalise(team);

        if (wanted.Length == 0)
        {
            return Fail("Name the team, or give its FM id.");
        }

        var exact = known.Where(t => Normalise(t.Name) == wanted).ToList();
        var close = exact.Count > 0
            ? exact
            : known.Where(t => Normalise(t.Name).Contains(wanted) || wanted.Contains(Normalise(t.Name))).ToList();

        return close.Count switch
        {
            1 => Result.Ok(close[0]),
            > 1 => Fail($"'{team}' matches several teams: {Describe(close)}. Use the full name or the id."),
            _ => Fail($"No team called '{team}' in the user's league or competitions (names are in the game's language). " +
                      $"Close ones: {Describe(Suggestions(wanted, known))}.")
        };
    }

    // Teams sharing a word start with the name given, else the user's league.
    private static List<KnownTeam> Suggestions(string wanted, List<KnownTeam> teams)
    {
        if (teams.Count == 0)
        {
            return [];
        }

        var words = Words(wanted).Where(word => word.Length >= 3).ToList();
        var similar = teams.Where(t => Words(Normalise(t.Name)).Any(word => words.Any(w => Prefix(word, w) >= 3))).ToList();

        return (similar.Count > 0 ? similar : teams.Where(t => t.Competition == teams[0].Competition).ToList())
            .Take(SuggestionsShown)
            .ToList();
    }

    private static TeamReport Report(TeamSeason season)
    {
        var competitive = season.Matches.Where(m => m.Summary.IsPlayed && !m.Summary.IsFriendly).ToList();
        var summaries = competitive.Select(m => m.Summary).ToList();

        return new TeamReport
        {
            TeamId = season.TeamId,
            TeamName = season.TeamName,
            Overall = MatchRecord.Of(summaries),
            Home = MatchRecord.Of(summaries.Where(m => m.Venue == Venue.Home)),
            Away = MatchRecord.Of(summaries.Where(m => m.Venue == Venue.Away)),
            Style = Profile(competitive),
            HomeStyle = Profile(competitive.Where(m => m.Summary.Venue == Venue.Home)),
            AwayStyle = Profile(competitive.Where(m => m.Summary.Venue == Venue.Away)),
            Lineup = Usual(competitive),
            Squad = season.Squad.OrderByDescending(p => p.SeasonStats?.Appearances ?? 0).ToList(),
            Played = summaries,
            Upcoming = season.Matches.Where(m => !m.Summary.IsPlayed).Select(m => m.Summary).ToList()
        };
    }

    // Only matches the game kept numbers for: it doesn't for every result it simulates away from the user.
    private static StyleProfile Profile(IEnumerable<TeamMatch> matches)
    {
        var withNumbers = matches.Where(m => m.Stats is not null && m.OpponentStats is not null).ToList();

        return new StyleProfile
        {
            Matches = withNumbers.Count,
            For = Average(withNumbers.Select(m => m.Stats!)),
            Against = Average(withNumbers.Select(m => m.OpponentStats!))
        };
    }

    private static StyleAverages Average(IEnumerable<TeamMatchStats> matches)
    {
        var stats = matches.ToList();

        double? Mean(Func<TeamMatchStats, double?> value)
        {
            var values = stats.Select(value).OfType<double>().ToList();
            return values.Count == 0 ? null : values.Average();
        }

        return new StyleAverages
        {
            Possession = Mean(s => s.Possession),
            ExpectedGoals = Mean(s => s.ExpectedGoals),
            Shots = Mean(s => s.Shots),
            ShotsOnTarget = Mean(s => s.ShotsOnTarget),
            LongShots = Mean(s => s.LongShots),
            ChancesCreated = Mean(s => s.ChancesCreated),
            Crosses = Mean(s => s.Crosses),
            CrossCompletionPercentage = Mean(s => s.CrossCompletionPercentage),
            Passes = Mean(s => s.Passes),
            PassCompletionPercentage = Mean(s => s.PassCompletionPercentage),
            ProgressivePasses = Mean(s => s.ProgressivePasses),
            FinalThirdPasses = Mean(s => s.FinalThirdPasses),
            PassesPerDefensiveAction = Mean(s => s.PassesPerDefensiveAction),
            Tackles = Mean(s => s.Tackles),
            HeaderCompletionPercentage = Mean(s => s.HeaderCompletionPercentage),
            Fouls = Mean(s => s.Fouls)
        };
    }

    private static UsualLineup Usual(List<TeamMatch> matches)
    {
        var starts = matches.SelectMany(m => m.Lineup.Where(entry => entry.IsStarter)).ToList();

        return new UsualLineup
        {
            InPossessionShapes = Counts(matches.Select(m => m.Summary.Formation)),
            OutOfPossessionShapes = Counts(matches.Select(m => m.Summary.OutOfPossessionFormation)),
            Players = starts
                .GroupBy(entry => entry.PlayerId?.ToString(CultureInfo.InvariantCulture) ?? entry.Name)
                .Where(group => group.Key is not null)
                .Select(group => new RegularPlayer
                {
                    PlayerId = group.First().PlayerId,
                    Name = group.First().Name,
                    Starts = group.Count(),
                    Position = MostCommon(group.Select(entry => entry.InPossessionPosition?.Code)),
                    Role = MostCommon(group.Select(entry => entry.InPossessionRole)),
                    OutOfPossessionPosition = MostCommon(group.Select(entry => entry.OutOfPossessionPosition?.Code)),
                    OutOfPossessionRole = MostCommon(group.Select(entry => entry.OutOfPossessionRole))
                })
                .OrderByDescending(player => player.Starts)
                .Take(RegularPlayersShown)
                .ToList()
        };
    }

    private static Dictionary<string, int> Counts(IEnumerable<string?> values)
    {
        return values.OfType<string>()
            .GroupBy(value => value)
            .OrderByDescending(group => group.Count())
            .ToDictionary(group => group.Key, group => group.Count());
    }

    private static string? MostCommon(IEnumerable<string?> values)
    {
        return values.OfType<string>().GroupBy(value => value).OrderByDescending(group => group.Count()).FirstOrDefault()?.Key;
    }

    private static Result<KnownTeam> Fail(string message)
    {
        return Result.Fail<KnownTeam>(new ValidationError("team", [message]));
    }

    private static string Describe(IEnumerable<KnownTeam> teams)
    {
        return string.Join(", ", teams.Select(t => $"{t.Name} ({t.Competition}, id {t.Id})"));
    }

    // Lower case without accents: "Atlético" and "atletico" are the same name.
    private static string Normalise(string name)
    {
        var decomposed = name.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var letters = decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(letters.ToArray()).Normalize(NormalizationForm.FormC);
    }

    private static IEnumerable<string> Words(string name)
    {
        return name.Split([' ', '-', '.', '\''], StringSplitOptions.RemoveEmptyEntries);
    }

    private static int Prefix(string a, string b)
    {
        var length = 0;

        while (length < a.Length && length < b.Length && a[length] == b[length])
        {
            length++;
        }

        return length;
    }
}
