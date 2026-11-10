using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Database;

namespace FootballManager.Application.Services;

public interface IDatabaseSearchService
{
    /// <summary>The game's search for a text: players, staff, clubs, competitions, nations, from the closest match down.</summary>
    Task<Result<DatabaseSearch>> SearchAsync(
        string? query, IReadOnlyCollection<DatabaseRecordKind>? kinds = null, int? limit = null, CancellationToken cancellationToken = default);
}

public class DatabaseSearchService(IDatabaseSearchGameClient gameClient) : IDatabaseSearchService
{
    public const int DefaultLimit = 10;
    public const int MaxLimit = 50;
    public const int MaxQueryLength = 100;

    public async Task<Result<DatabaseSearch>> SearchAsync(
        string? query, IReadOnlyCollection<DatabaseRecordKind>? kinds = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var text = query?.Trim() ?? string.Empty;

        if (text.Length == 0)
        {
            return Result.Fail<DatabaseSearch>(new ValidationError("query", ["Give a text to search for."]));
        }

        if (text.Length > MaxQueryLength)
        {
            return Result.Fail<DatabaseSearch>(new ValidationError("query", [$"The text runs to {MaxQueryLength} characters at most."]));
        }

        if (limit is < 1 or > MaxLimit)
        {
            return Result.Fail<DatabaseSearch>(new ValidationError("limit", [$"The limit runs 1 to {MaxLimit}."]));
        }

        try
        {
            return Result.Ok(await gameClient.SearchAsync(text, kinds ?? [], limit ?? DefaultLimit, cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<DatabaseSearch>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<DatabaseSearch>(new ValidationError("game", [exception.Message]));
        }
    }
}
