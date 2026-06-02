using FluentAssertions;
using GbfsQuiz.Application.Features.Leaderboard;
using GbfsQuiz.Application.Features.Leaderboard.Interfaces;
using GbfsQuiz.Application.Features.Leaderboard.Models;
using Moq;
using Xunit;

namespace GbfsQuiz.Tests.Leaderboard;

public sealed class LeaderboardServiceTests
{
    private readonly Mock<ILeaderboardRepository> _repository = new();

    [Fact]
    public async Task GetTopAsync_AssignsSequentialRanksInRepositoryOrder()
    {
        _repository.Setup(r => r.GetTopAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeaderboardStanding>
            {
                new(Guid.CreateVersion7(), "Ada", false, 300, 3, 4),
                new(Guid.CreateVersion7(), "Linus", true, 150, 1, 2)
            });
        var service = new LeaderboardService(_repository.Object);

        var result = await service.GetTopAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(e => e.Rank).Should().ContainInOrder(1, 2);
        result.Value[0].DisplayName.Should().Be("Ada");
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(999, 50)]
    public async Task GetTopAsync_ClampsLimitBetweenOneAndFifty(int requested, int expected)
    {
        _repository.Setup(r => r.GetTopAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var service = new LeaderboardService(_repository.Object);

        await service.GetTopAsync(requested);

        _repository.Verify(r => r.GetTopAsync(expected, It.IsAny<CancellationToken>()), Times.Once);
    }
}
