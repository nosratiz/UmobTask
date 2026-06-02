using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GbfsQuiz.Web.Common.Security;

/// <summary>Reads the authenticated player's id from the JWT <c>sub</c> claim.</summary>
public static class CurrentPlayer
{
    public static bool TryGetId(this ClaimsPrincipal principal, out Guid playerId)
    {
        var raw = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out playerId);
    }
}
