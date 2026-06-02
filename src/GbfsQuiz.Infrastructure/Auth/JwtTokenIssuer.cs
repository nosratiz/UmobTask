using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Domain.Players;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GbfsQuiz.Infrastructure.Auth;

/// <summary>Issues short-lived HMAC-signed JWTs carrying the player id as <c>sub</c>.</summary>
public sealed class JwtTokenIssuer(IOptions<JwtOptions> options) : ITokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public TokenResult Issue(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiryMinutes);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: BuildClaims(player),
            expires: expiresAt.UtcDateTime,
            signingCredentials: BuildCredentials());

        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static Claim[] BuildClaims(Player player) =>
    [
        new(JwtRegisteredClaimNames.Sub, player.Id.ToString()),
        new(JwtRegisteredClaimNames.UniqueName, player.Username),
        new("display_name", player.DisplayName),
        new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
    ];

    private SigningCredentials BuildCredentials()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}
