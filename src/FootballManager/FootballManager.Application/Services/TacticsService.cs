using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Tactics;

namespace FootballManager.Application.Services;

public interface ITacticsService
{
    Task<Result<TeamTactics>> GetTacticsAsync(CancellationToken cancellationToken = default);

    Task<Result<RoleFit>> GetRoleFitAsync(
        string position, bool inPossession, int top = TacticsService.DefaultPlayersPerRole, CancellationToken cancellationToken = default);
}

public class TacticsService(ITacticsGameClient gameClient) : ITacticsService
{
    public const int DefaultPlayersPerRole = 5;
    public const int MaxPlayersPerRole = 40;

    public async Task<Result<TeamTactics>> GetTacticsAsync(CancellationToken cancellationToken = default)
    {
        TeamTactics tactics;

        try
        {
            tactics = await gameClient.GetTacticsAsync(cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<TeamTactics>(new GameUnavailableError(exception.Message));
        }

        foreach (var assignment in tactics.Active.Lineup.SelectMany(p => new[] { p.InPossession, p.OutOfPossession }))
        {
            assignment.RoleOptions = BestFirst(assignment.RoleOptions, r => r.Rating).ToList();
        }

        return Result.Ok(tactics);
    }

    public async Task<Result<RoleFit>> GetRoleFitAsync(
        string position, bool inPossession, int top = DefaultPlayersPerRole, CancellationToken cancellationToken = default)
    {
        // Enum.TryParse also takes numbers, which aren't positions.
        if (!Enum.TryParse<PitchPosition>(position, ignoreCase: true, out var pitchPosition) || int.TryParse(position, out _))
        {
            return Result.Fail<RoleFit>(new ValidationError(
                nameof(position), [$"Unknown position '{position}'. Use one of {string.Join(", ", Enum.GetNames<PitchPosition>())}."]));
        }

        if (top is < 1 or > MaxPlayersPerRole)
        {
            return Result.Fail<RoleFit>(new ValidationError(nameof(top), [$"Must be between 1 and {MaxPlayersPerRole}."]));
        }

        RoleFit fit;

        try
        {
            fit = await gameClient.GetRoleFitAsync(pitchPosition, inPossession, cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<RoleFit>(new GameUnavailableError(exception.Message));
        }

        foreach (var role in fit.Roles)
        {
            role.Players = BestFirst(role.Players, p => p.Rating).Take(top).ToList();
        }

        return Result.Ok(fit);
    }

    private static IEnumerable<T> BestFirst<T>(IEnumerable<T> items, Func<T, Rating?> rating)
    {
        return items.OrderByDescending(i => rating(i)?.Max ?? -1).ThenByDescending(i => rating(i)?.Min ?? -1);
    }
}
