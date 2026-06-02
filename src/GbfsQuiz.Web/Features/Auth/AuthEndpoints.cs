using GbfsQuiz.Web.Common.Http;
using GbfsQuiz.Web.Common.Validation;
using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Application.Features.Auth.Requests;

namespace GbfsQuiz.Web.Features.Auth;

/// <summary>Minimal-API endpoints for account creation and login.</summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").RequireRateLimiting("auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService auth, CancellationToken ct) =>
            {
                var result = await auth.RegisterAsync(request.Username, request.DisplayName, request.Password, ct);
                return result.ToHttpResult();
            })
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
            .AllowAnonymous()
            .WithName("Register");

        group.MapPost("/login", async (LoginRequest request, IAuthService auth, CancellationToken ct) =>
            {
                var result = await auth.LoginAsync(request.Username, request.Password, ct);
                return result.ToHttpResult();
            })
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .AllowAnonymous()
            .WithName("Login");

        return app;
    }
}
