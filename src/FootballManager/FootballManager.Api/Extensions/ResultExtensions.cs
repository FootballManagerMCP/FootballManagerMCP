using FluentResults;
using FootballManager.Api.Resources;
using FootballManager.Application.ResultErrors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Extensions;

public static class ResultExtensions
{
    public static Results<Ok<TResponseModel>, BadRequest, NotFound, ProblemHttpResult> ToGetResult<TDomainModel, TResponseModel>(
        this Result<TDomainModel> serviceResult, Func<TDomainModel, TResponseModel> mapper)
    {
        if (serviceResult.IsSuccess)
        {
            return TypedResults.Ok(mapper(serviceResult.Value));
        }

        if (serviceResult.HasError<ValidationError>(out var validationErrors))
        {
            return CreateBadRequestResult(validationErrors);
        }

        if (serviceResult.HasError<EntityDoesNotExistError>())
        {
            return TypedResults.NotFound();
        }

        // The game being closed or busy is expected; say why so the caller can tell the user.
        if (serviceResult.HasError<GameUnavailableError>(out var unavailable))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: ResponseMessages.Status503Title,
                detail: unavailable.First().Message);
        }

        return TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    private static ProblemHttpResult CreateBadRequestResult(IEnumerable<ValidationError> validationErrors)
    {
        var errorsDictionary = validationErrors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(x => x.ErrorMessages).ToArray()
            );

        return TypedResults.Problem(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ResponseMessages.Status400Title,
            Extensions = new Dictionary<string, object?> {
                { ResponseMessages.Status400JsonErrorsTitle, errorsDictionary }
            }
        });
    }
}
