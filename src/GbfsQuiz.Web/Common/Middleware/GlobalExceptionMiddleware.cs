using System.Diagnostics;

namespace GbfsQuiz.Web.Common.Middleware;

/// <summary>
/// Last line of defence: catches unhandled exceptions, logs them with Serilog
/// using structured templates, and returns an RFC 7807 ProblemDetails payload.
/// Expected business failures should travel through FluentResults, not here.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        logger.LogError(ex,
            "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
            context.Request.Method, context.Request.Path, traceId);

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(BuildProblem(traceId));
    }

    private static object BuildProblem(string traceId) => new
    {
        type = "https://datatracker.ietf.org/doc/html/rfc7807",
        title = "An unexpected error occurred.",
        status = StatusCodes.Status500InternalServerError,
        traceId
    };
}
