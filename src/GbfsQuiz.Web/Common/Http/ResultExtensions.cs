using FluentResults;
using GbfsQuiz.Application.Common.Errors;

namespace GbfsQuiz.Web.Common.Http;

/// <summary>
/// Central translation of FluentResults into HTTP responses for Minimal API
/// endpoints. Keeps endpoints exception-free: business failures are values,
/// not thrown exceptions.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : MapFailure(result.Errors);

    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? Results.NoContent() : MapFailure(result.Errors);

    public static IResult ToCreatedResult<T>(this Result<T> result, string location) =>
        result.IsSuccess ? Results.Created(location, result.Value) : MapFailure(result.Errors);

    private static IResult MapFailure(IReadOnlyList<IError> errors)
    {
        var error = errors[0];
        return error switch
        {
            NotFoundError => Results.Problem(error.Message, statusCode: StatusCodes.Status404NotFound),
            ConflictError => Results.Problem(error.Message, statusCode: StatusCodes.Status409Conflict),
            UnauthorizedError => Results.Problem(error.Message, statusCode: StatusCodes.Status401Unauthorized),
            ExternalServiceError => Results.Problem(error.Message, statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest)
        };
    }
}
