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
            limiter.AddFixedWindowLimiter("auth", o => Configure(o, permit: 10));
            limiter.AddFixedWindowLimiter("game", o => Configure(o, permit: 120));
            limiter.AddFixedWindowLimiter("public", o => Configure(o, permit: 60));
        });
        return services;
    }

    private static void Configure(FixedWindowRateLimiterOptions options, int permit)
    {
        options.PermitLimit = permit;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
    }
}
