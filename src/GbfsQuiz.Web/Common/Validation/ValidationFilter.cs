using FluentValidation;

namespace GbfsQuiz.Web.Common.Validation;

/// <summary>
/// Endpoint filter that validates the request body of type <typeparamref name="T"/>
/// using its FluentValidation validator, returning RFC 7807 ValidationProblem on failure.
/// </summary>
public sealed class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
        {
            return Results.BadRequest("Request body is required.");
        }

        var result = await validator.ValidateAsync(model);
        return result.IsValid ? await next(context) : Results.ValidationProblem(ToErrors(result));
    }

    private static Dictionary<string, string[]> ToErrors(FluentValidation.Results.ValidationResult result) =>
        result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}
