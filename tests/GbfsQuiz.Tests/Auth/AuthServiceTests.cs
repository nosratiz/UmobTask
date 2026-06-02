using FluentAssertions;
using FluentResults;
using GbfsQuiz.Application.Features.Auth;
using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Domain.Players;
using Moq;
using Xunit;

namespace GbfsQuiz.Tests.Auth;

public sealed class AuthServiceTests
{
    private readonly Mock<IPlayerRepository> _players = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenIssuer> _tokens = new();

    public AuthServiceTests()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _tokens.Setup(t => t.Issue(It.IsAny<Player>()))
            .Returns(new TokenResult("jwt", DateTimeOffset.UtcNow.AddHours(2)));
    }

    private AuthService CreateService() => new(_players.Object, _hasher.Object, _tokens.Object);

    [Fact]
    public async Task RegisterAsync_WithFreeUsername_CreatesPlayerAndIssuesToken()
    {
        _players.Setup(p => p.ExistsAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateService().RegisterAsync("Alice", "Alice A", "password123");

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt");
        _players.Verify(p => p.CreateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithTakenUsername_Fails()
    {
        _players.Setup(p => p.ExistsAsync("bob", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateService().RegisterAsync("Bob", "Bob", "password123");

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_FailsWithoutLeakingDetail()
    {
        var player = new Player("carol", "Carol", "stored-hash");
        _players.Setup(p => p.FindByUsernameAsync("carol", It.IsAny<CancellationToken>())).ReturnsAsync(player);
        _hasher.Setup(h => h.Verify("stored-hash", "bad")).Returns(false);

        var result = await CreateService().LoginAsync("Carol", "bad");

        result.IsFailed.Should().BeTrue();
    }
}
