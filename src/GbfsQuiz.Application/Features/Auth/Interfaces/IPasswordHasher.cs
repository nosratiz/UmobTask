namespace GbfsQuiz.Application.Features.Auth.Interfaces;

/// <summary>Abstracts credential hashing so the domain never sees the algorithm.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string providedPassword);
}
