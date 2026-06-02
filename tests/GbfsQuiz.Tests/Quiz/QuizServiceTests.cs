using FluentAssertions;
using FluentResults;
using GbfsQuiz.Application.Features.Gbfs.Interfaces;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Strategies;
using GbfsQuiz.Infrastructure.Quiz;
using GbfsQuiz.Tests.Common;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace GbfsQuiz.Tests.Quiz;

public sealed class QuizServiceTests
{
    private readonly Mock<IGbfsSnapshotProvider> _snapshots = new();
    private readonly IIssuedQuestionStore _store =
        new MemoryIssuedQuestionStore(new MemoryCache(new MemoryCacheOptions()));

    private QuizService CreateService() =>
        new(_snapshots.Object, AllStrategies(), _store);

    private static IEnumerable<IQuestionStrategy> AllStrategies() =>
    [
        new MostBikesCityStrategy(), new BiggestNetworkStrategy(),
        new BikesInCityStrategy(), new StationCountStrategy(), new NearestStationStrategy()
    ];

    private void GivenSnapshots(params GbfsSnapshot[] snapshots) =>
        _snapshots.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<GbfsSnapshot>>(snapshots));

    [Fact]
    public async Task NextQuestionAsync_WithRichData_ReturnsAnswerableQuestion()
    {
        GivenSnapshots(
            SnapshotBuilder.Snapshot("a", "Alpha", 20),
            SnapshotBuilder.Snapshot("b", "Bravo", 40));

        var result = await CreateService().NextQuestionAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Choices.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Grade_CorrectChoice_ScoresAsCorrect()
    {
        GivenSnapshots(SnapshotBuilder.Snapshot("a", "Alpha", 30));
        var service = CreateService();
        var question = (await service.NextQuestionAsync()).Value;

        var correct = service.Grade(question.Id, question.CorrectChoiceId).Value;

        correct.Correct.Should().BeTrue();
        correct.CorrectChoiceId.Should().Be(question.CorrectChoiceId);
    }

    [Fact]
    public async Task Grade_WrongChoice_ScoresAsIncorrect()
    {
        GivenSnapshots(SnapshotBuilder.Snapshot("a", "Alpha", 30));
        var service = CreateService();
        var question = (await service.NextQuestionAsync()).Value;
        var wrong = question.Choices.First(c => c.Id != question.CorrectChoiceId).Id;

        service.Grade(question.Id, wrong).Value.Correct.Should().BeFalse();
    }

    [Fact]
    public async Task Grade_SameQuestionTwice_FailsSecondTime_PreventingReplay()
    {
        GivenSnapshots(SnapshotBuilder.Snapshot("a", "Alpha", 30));
        var service = CreateService();
        var question = (await service.NextQuestionAsync()).Value;

        service.Grade(question.Id, question.CorrectChoiceId).IsSuccess.Should().BeTrue();

        // A question is single-use: regrading it (the score-farming vector) must fail.
        service.Grade(question.Id, question.CorrectChoiceId).IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Grade_ForUnknownQuestion_Fails()
    {
        var result = CreateService().Grade(Guid.CreateVersion7(), Guid.CreateVersion7());

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task NextQuestionAsync_WhenSnapshotProviderFails_PropagatesFailure()
    {
        _snapshots.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyList<GbfsSnapshot>>("upstream down"));

        var result = await CreateService().NextQuestionAsync();

        result.IsFailed.Should().BeTrue();
    }
}
