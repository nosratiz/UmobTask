using FluentAssertions;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz.Strategies;
using GbfsQuiz.Tests.Common;
using Xunit;

namespace GbfsQuiz.Tests.Quiz;

public sealed class QuestionStrategyTests
{
    private static readonly Random Rng = new(42);

    [Fact]
    public void MostBikesCityStrategy_MarksCityWithMostBikesAsCorrect()
    {
        var snapshots = new List<GbfsSnapshot>
        {
            SnapshotBuilder.Snapshot("a", "Quiet Town", stationCount: 5, bikesPerStation: 1),
            SnapshotBuilder.Snapshot("b", "Bike City", stationCount: 5, bikesPerStation: 9)
        };
        var strategy = new MostBikesCityStrategy();

        var question = strategy.Generate(snapshots, Rng).Value;

        var correct = question.Choices.Single(c => c.Id == question.CorrectChoiceId);
        correct.Text.Should().Be("Bike City");
        question.Choices.Should().HaveCount(2);
    }

    [Fact]
    public void BiggestNetworkStrategy_MarksProviderWithMostStationsAsCorrect()
    {
        var snapshots = new List<GbfsSnapshot>
        {
            SnapshotBuilder.Snapshot("a", "Small", stationCount: 3),
            SnapshotBuilder.Snapshot("b", "Large", stationCount: 30)
        };
        var strategy = new BiggestNetworkStrategy();

        var question = strategy.Generate(snapshots, Rng).Value;

        question.Choices.Single(c => c.Id == question.CorrectChoiceId).Text.Should().Contain("Large");
    }

    [Fact]
    public void NearestStationStrategy_RequiresFourStations()
    {
        var strategy = new NearestStationStrategy();
        var tooFew = new List<GbfsSnapshot> { SnapshotBuilder.Snapshot("a", "Tiny", stationCount: 3) };
        var enough = new List<GbfsSnapshot> { SnapshotBuilder.Snapshot("a", "Big", stationCount: 10) };

        strategy.CanGenerate(tooFew).Should().BeFalse();
        strategy.CanGenerate(enough).Should().BeTrue();
    }

    [Fact]
    public void NumericDistractorGenerator_ProducesRequestedDistinctNonCorrectValues()
    {
        var distractors = NumericDistractorGenerator.Generate(correct: 100, count: 3, Rng);

        distractors.Should().HaveCount(3);
        distractors.Should().OnlyHaveUniqueItems();
        distractors.Should().NotContain("100");
    }
}
