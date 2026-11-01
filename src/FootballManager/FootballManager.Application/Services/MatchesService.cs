using System.Globalization;
using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Matches;

namespace FootballManager.Application.Services;

public interface IMatchesService
{
    Task<Result<SeasonMatches>> GetMatchesAsync(bool includeFriendlies = false, CancellationToken cancellationToken = default);

    Task<Result<MatchDetail>> GetMatchAsync(string date, CancellationToken cancellationToken = default);
}

public class MatchesService(IMatchesGameClient gameClient) : IMatchesService
{
    public const string DateFormat = "yyyy-MM-dd";

    public async Task<Result<SeasonMatches>> GetMatchesAsync(
        bool includeFriendlies = false, CancellationToken cancellationToken = default)
    {
        TeamFixtures fixtures;

        try
        {
            fixtures = await gameClient.GetFixturesAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<SeasonMatches>(new GameUnavailableError(exception.Message));
        }

        // Friendlies never count towards the record; they're only listed when asked for.
        var competitive = fixtures.Fixtures.Where(f => f.IsPlayed && !f.IsFriendly).ToList();

        return Result.Ok(new SeasonMatches
        {
            TeamName = fixtures.TeamName,
            Overall = MatchRecord.Of(competitive),
            Home = MatchRecord.Of(competitive.Where(f => f.Venue == Venue.Home)),
            Away = MatchRecord.Of(competitive.Where(f => f.Venue == Venue.Away)),
            Played = fixtures.Fixtures.Where(f => f.IsPlayed && (includeFriendlies || !f.IsFriendly)).ToList(),
            Upcoming = fixtures.Fixtures.Where(f => !f.IsPlayed).ToList()
        });
    }

    public async Task<Result<MatchDetail>> GetMatchAsync(string date, CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParseExact(date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
        {
            return Result.Fail<MatchDetail>(new ValidationError(nameof(date), [$"Use the match date as {DateFormat}, e.g. 2026-10-11."]));
        }

        MatchDetail? match;

        try
        {
            match = await gameClient.GetMatchAsync(day, cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<MatchDetail>(new GameUnavailableError(exception.Message));
        }

        return match is null ? Result.Fail<MatchDetail>(new EntityDoesNotExistError()) : Result.Ok(match);
    }
}
