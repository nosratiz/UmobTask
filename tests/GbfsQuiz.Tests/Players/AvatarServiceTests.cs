using FluentAssertions;
using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Application.Features.Players;
using GbfsQuiz.Application.Features.Players.Interfaces;
using GbfsQuiz.Domain.Players;
using Moq;
using Xunit;

namespace GbfsQuiz.Tests.Players;

public sealed class AvatarServiceTests
{
    private readonly Mock<IPlayerRepository> _players = new();
    private readonly Mock<IAvatarStorage> _storage = new();

    private AvatarService CreateService() => new(_players.Object, _storage.Object);

    [Fact]
    public async Task SetAvatarAsync_WithValidPng_StoresFileAndSavesPath()
    {
        var player = new Player("ada", "Ada", "hash");
        _players.Setup(p => p.GetByIdAsync(player.Id, It.IsAny<CancellationToken>())).ReturnsAsync(player);
        _storage
            .Setup(s => s.SaveAsync(It.IsAny<byte[]>(), "image/png", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("/avatars/abc.png");
        var service = CreateService();

        var result = await service.SetAvatarAsync(player.Id, [1, 2, 3, 4], "image/png");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("/avatars/abc.png");
        player.AvatarPath.Should().Be("/avatars/abc.png");
        player.HasAvatar.Should().BeTrue();
        _players.Verify(p => p.UpdateAsync(player, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAvatarAsync_WithDisallowedType_Fails()
    {
        var service = CreateService();

        var result = await service.SetAvatarAsync(Guid.CreateVersion7(), [1, 2, 3], "application/pdf");

        result.IsFailed.Should().BeTrue();
        _players.Verify(p => p.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _storage.Verify(
            s => s.SaveAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAvatarAsync_WhenTooLarge_Fails()
    {
        var service = CreateService();
        var tooBig = new byte[(512 * 1024) + 1];

        var result = await service.SetAvatarAsync(Guid.CreateVersion7(), tooBig, "image/png");

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetAvatarUrlAsync_WhenPlayerHasNoAvatar_ReturnsNotFound()
    {
        var player = new Player("ada", "Ada", "hash");
        _players.Setup(p => p.GetByIdAsync(player.Id, It.IsAny<CancellationToken>())).ReturnsAsync(player);
        var service = CreateService();

        var result = await service.GetAvatarUrlAsync(player.Id);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetAvatarUrlAsync_WhenPlayerHasAvatar_ReturnsPath()
    {
        var player = new Player("ada", "Ada", "hash");
        player.SetAvatar("/avatars/abc.png");
        _players.Setup(p => p.GetByIdAsync(player.Id, It.IsAny<CancellationToken>())).ReturnsAsync(player);
        var service = CreateService();

        var result = await service.GetAvatarUrlAsync(player.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("/avatars/abc.png");
    }
}
