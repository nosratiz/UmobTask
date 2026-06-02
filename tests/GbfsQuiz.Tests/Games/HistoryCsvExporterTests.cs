using FluentAssertions;
using GbfsQuiz.Application.Features.Games;
using GbfsQuiz.Application.Features.Games.Responses;
using Xunit;

namespace GbfsQuiz.Tests.Games;

public sealed class HistoryCsvExporterTests
{
    [Fact]
    public void ToCsv_WritesHeaderAndOneRowPerGame()
    {
        var history = new List<GameSummaryResponse>
        {
            new(Guid.CreateVersion7(), 80, "Won", 2, 0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new(Guid.CreateVersion7(), -20, "Lost", 0, 1, DateTimeOffset.UtcNow, null)
        };

        var csv = HistoryCsvExporter.ToCsv(history);

        var lines = csv.TrimEnd().Split(Environment.NewLine);
        lines.Should().HaveCount(3);
        lines[0].Should().StartWith("GameId,Outcome,Score");
        lines[1].Should().Contain("Won").And.Contain("80");
        lines[2].Should().Contain("Lost");
    }

    [Fact]
    public void ToCsv_WithNoGames_WritesHeaderOnly()
    {
        var csv = HistoryCsvExporter.ToCsv([]);

        csv.TrimEnd().Split(Environment.NewLine).Should().ContainSingle();
    }
}
