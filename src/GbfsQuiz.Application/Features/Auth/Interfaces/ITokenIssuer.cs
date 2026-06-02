using GbfsQuiz.Domain.Players;

namespace GbfsQuiz.Application.Features.Auth.Interfaces;

/// <summary>Issues a bearer token authenticating a player to the game endpoints.</summary>
public interface ITokenIssuer
{
    TokenResult Issue(Player player);
}
