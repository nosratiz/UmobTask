namespace GbfsQuiz.Infrastructure.Auth;

/// <summary>Bound from the <c>Jwt</c> configuration section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "GbfsQuiz";
    public string Audience { get; init; } = "GbfsQuiz";

    /// <summary>HMAC signing key. Must be at least 32 chars; override in real deployments.</summary>
    public string SigningKey { get; init; } = "dev-only-signing-key-change-me-please-32+chars";
    public int ExpiryMinutes { get; init; } = 120;
}
