using System.Globalization;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Text;
using FootballManager.Domain.Competitions;

namespace FootballManager.Application.Services;

public interface ICompetitionsService
{
    /// <summary>The leagues whose stats screens can be read: the user's, and those the game plays out nearby.</summary>
    Task<Result<List<CompetitionInfo>>> GetCompetitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>A competition's players ranked by a stat, narrowed down, each cross-checked with scouting and contract.</summary>
    Task<Result<CompetitionPlayerStats>> GetPlayerStatsAsync(CompetitionStatsQuery query, CancellationToken cancellationToken = default);
}

// A competition's stats screen ranks its players one stat at a time, keeping the best 100 of each. The stat players are
// ranked by picks them; the other stats read are added where a player is in their tables too. Only the players kept are
// read in full: what the user's scouting knows of them, and their contracts.
public class CompetitionsService(ICompetitionsGameClient gameClient) : ICompetitionsService
{
    public const int MaxStats = 10;
    public const int MaxLimit = 50;
    public const int MinAge = 14;
    public const int MaxAge = 50;

    public async Task<Result<List<CompetitionInfo>>> GetCompetitionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Ok(await gameClient.GetCompetitionsAsync(cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<List<CompetitionInfo>>(new GameUnavailableError(exception.Message));
        }
    }

    public async Task<Result<CompetitionPlayerStats>> GetPlayerStatsAsync(
        CompetitionStatsQuery query, CancellationToken cancellationToken = default)
    {
        var errors = Check(query);

        if (errors.Count > 0)
        {
            return Result.Fail<CompetitionPlayerStats>(errors);
        }

        try
        {
            var competition = await ResolveAsync(query.Competition, cancellationToken);

            if (competition.IsFailed)
            {
                return competition.ToResult<CompetitionPlayerStats>();
            }

            var sortBy = query.SortBy ?? query.Stats[0];
            var stats = query.Stats.Prepend(sortBy).Distinct().ToList();
            var tables = await Task.WhenAll(stats.Select(stat => gameClient.GetStatTableAsync(competition.Value.Id, stat, cancellationToken)));
            var userTeam = query.IncludeOwnPlayers ? null : await gameClient.GetUserTeamIdAsync(cancellationToken);

            var matched = tables[0].Rows.Where(row => Keeps(row, query, userTeam)).ToList();
            var picked = matched.Take(query.Limit).ToList();
            var profiles = await gameClient.GetPlayersAsync(
                competition.Value.Id,
                picked.Select(row => new CompetitionRowReference(sortBy, row.RowIndex)).ToList(),
                cancellationToken);

            return Result.Ok(new CompetitionPlayerStats
            {
                Competition = competition.Value,
                Stats = tables.Select(table => new CompetitionStatColumn { Stat = table.Stat, Label = table.Label }).ToList(),
                SortedBy = sortBy,
                Matched = matched.Count,
                Players = picked.Select((row, index) => Player(row, profiles[index], tables)).ToList()
            });
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<CompetitionPlayerStats>(new GameUnavailableError(exception.Message));
        }
    }

    internal static List<IError> Check(CompetitionStatsQuery query)
    {
        var errors = new List<IError>();

        if (query.Stats.Count == 0)
        {
            errors.Add(new ValidationError(nameof(query.Stats), ["Name at least one stat to read."]));
        }

        if (query.Stats.Distinct().Count() > MaxStats)
        {
            errors.Add(new ValidationError(nameof(query.Stats), [$"At most {MaxStats} stats at a time."]));
        }

        if (query.Limit is < 1 or > MaxLimit)
        {
            errors.Add(new ValidationError(nameof(query.Limit), [$"The limit runs from 1 to {MaxLimit}."]));
        }

        if (query.MinAge is < MinAge or > MaxAge || query.MaxAge is < MinAge or > MaxAge)
        {
            errors.Add(new ValidationError("age", [$"Ages run from {MinAge} to {MaxAge}."]));
        }

        if (query is { MinAge: { } youngest, MaxAge: { } oldest } && youngest > oldest)
        {
            errors.Add(new ValidationError("age", ["The minimum age is above the maximum."]));
        }

        if (query.MinMinutes is < 0)
        {
            errors.Add(new ValidationError(nameof(query.MinMinutes), ["Minutes can't be negative."]));
        }

        return errors;
    }

    // The user's league unless another is named: by FM id, by name (ignoring case and accents), or by part of a name.
    private async Task<Result<CompetitionInfo>> ResolveAsync(string? competition, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(competition))
        {
            return Result.Ok(await gameClient.GetUserLeagueAsync(cancellationToken));
        }

        return Find(competition, await gameClient.GetCompetitionsAsync(cancellationToken));
    }

    internal static Result<CompetitionInfo> Find(string competition, List<CompetitionInfo> competitions)
    {
        if (int.TryParse(competition, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            && competitions.FirstOrDefault(c => c.Id == id) is { } byId)
        {
            return Result.Ok(byId);
        }

        var wanted = GameNames.Normalise(competition);
        var exact = competitions.Where(c => GameNames.Normalise(c.Name) == wanted).ToList();
        var close = exact.Count > 0 ? exact : competitions.Where(c => GameNames.Normalise(c.Name).Contains(wanted)).ToList();

        return close.Count switch
        {
            1 => Result.Ok(close[0]),
            > 1 => Fail($"'{competition}' matches several competitions: {Describe(close)}. Use the full name or the id."),
            _ => Fail($"No competition called '{competition}' with player stats (names are in the game's language). " +
                      $"Those that have them: {Describe(competitions)}.")
        };
    }

    // The sort stat's table is in the game's order, best first; filters only narrow it down.
    private static bool Keeps(CompetitionStatRow row, CompetitionStatsQuery query, int? userTeam)
    {
        if (userTeam is not null && row.TeamId == userTeam)
        {
            return false;
        }

        if ((query.MinAge is not null || query.MaxAge is not null) && row.Age is null)
        {
            return false;
        }

        if (row.Age < query.MinAge || row.Age > query.MaxAge)
        {
            return false;
        }

        if (query.Positions.Count > 0 && !row.Positions.Intersect(query.Positions).Any())
        {
            return false;
        }

        return query.MinMinutes is not { } minutes || row.Minutes >= minutes;
    }

    private static CompetitionPlayer Player(CompetitionStatRow row, CompetitionPlayerProfile profile, CompetitionStatTable[] tables)
    {
        return new CompetitionPlayer
        {
            Player = profile.Player,
            Contract = profile.Contract,
            TeamId = row.TeamId,
            Positions = row.Positions,
            Minutes = row.Minutes,
            Stats = tables
                .Select(table => table.Rows.FirstOrDefault(other => other.PlayerId == row.PlayerId) is { } found
                    ? new CompetitionStatValue { Stat = table.Stat, Value = found.Value, Rank = found.Rank }
                    : null)
                .OfType<CompetitionStatValue>()
                .ToList()
        };
    }

    private static Result<CompetitionInfo> Fail(string message)
    {
        return Result.Fail<CompetitionInfo>(new ValidationError("competition", [message]));
    }

    private static string Describe(IEnumerable<CompetitionInfo> competitions)
    {
        return string.Join(", ", competitions.Select(c => $"{c.Name} ({c.Nation}, reputation {c.Reputation}, id {c.Id})"));
    }
}
