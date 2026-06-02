using System.Text;
using System.Threading.RateLimiting;
using GbfsQuiz.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace GbfsQuiz.Web.Common.Security;

/// <summary>Configures JWT bearer authentication and per-feature rate limiting.</summary>
public static class SecuritySetup
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(bearer => bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddGameRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Each policy is PARTITIONED so the limit applies per client, not as one global
            // bucket shared by everyone. "game" is keyed by the authenticated player id (so
            // one player can't throttle the rest); "auth"/"public" are pre-auth, so they key
            // by client IP (requires UseForwardedHeaders behind the reverse proxy).
            limiter.AddPolicy("auth", ctx => PerClientIp(ctx, permit: 10));
            limiter.AddPolicy("game", ctx => PerPlayer(ctx, permit: 120));
            limiter.AddPolicy("public", ctx => PerClientIp(ctx, permit: 60));
        });
        return services;
    }

    private static RateLimitPartition<string> PerPlayer(HttpContext ctx, int permit)
    {
        var key = ctx.User.TryGetId(out var playerId)
            ? $"player:{playerId}"
            : $"ip:{ClientIp(ctx)}"; // fall back to IP if somehow unauthenticated
        return Window(key, permit);
    }

    private static RateLimitPartition<string> PerClientIp(HttpContext ctx, int permit) =>
        Window($"ip:{ClientIp(ctx)}", permit);

    private static string ClientIp(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static RateLimitPartition<string> Window(string key, int permit) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
}
