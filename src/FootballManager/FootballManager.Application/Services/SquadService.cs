using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Attributes;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Services;

public interface ISquadService
{
    Task<Result<Squad>> GetSquadAsync(
        bool includeAttributes = false, bool includeHidden = false, CancellationToken cancellationToken = default);
}

public class SquadService(IGameClient gameClient) : ISquadService
{
    public async Task<Result<Squad>> GetSquadAsync(
        bool includeAttributes = false, bool includeHidden = false, CancellationToken cancellationToken = default)
    {
        // Hidden values are never fetched unless asked for, so they can't leak into an answer by accident.
        var attributes = new List<AttributeDefinition>();

        if (includeAttributes)
        {
            attributes.AddRange(PlayerAttributes.Visible);
        }

        if (includeHidden)
        {
            attributes.AddRange(PlayerAttributes.Hidden);
        }

        Squad squad;

        try
        {
            squad = await gameClient.GetSquadAsync(attributes, includeHidden, cancellationToken);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<Squad>(new GameUnavailableError(exception.Message));
        }

        squad.HiddenDataMasked = includeHidden && !squad.Players.Any(p => p.HasHiddenValues);
        return Result.Ok(squad);
    }
}
