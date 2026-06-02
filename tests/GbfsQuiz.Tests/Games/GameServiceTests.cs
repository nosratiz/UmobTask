using FluentAssertions;
using FluentResults;
using GbfsQuiz.Application.Features.Games;
using GbfsQuiz.Application.Features.Games.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;
using GbfsQuiz.Domain.Games;
using Moq;
using Xunit;

namespace GbfsQuiz.Tests.Games;

public sealed class GameServiceTests
{
    private readonly Mock<IGameSessionRepository> _repo = new();
    private readonly Mock<IQuizService> _quiz = new();
    private readonly Guid _playerId = Guid.CreateVersion7();

    private GameService CreateService() => new(_repo.Object, _quiz.Object);

    private static QuizQuestion AnyQuestion()
    {
        var correct = new QuizChoice(Guid.CreateVersion7(), "Right");
        var wrong = new QuizChoice(Guid.CreateVersion7(), "Wrong");
        return new QuizQuestion(Guid.CreateVersion7(), "Test", "Q?", [correct, wrong], correct.Id);
    }

    private void QuizReturnsQuestion() =>
        _quiz.Setup(q => q.NextQuestionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(AnyQuestion()));

    [Fact]
    public async Task StartAsync_PersistsSessionAndReturnsFirstQuestion()
    {
        QuizReturnsQuestion();

        var result = await CreateService().StartAsync(_playerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Question.Choices.Should().HaveCount(2);
        _repo.Verify(r => r.CreateAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswerAsync_ForUnknownGame_ReturnsFailure()
    {
        _repo.Setup(r => r.GetForPlayerAsync(It.IsAny<Guid>(), _playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameSession?)null);

        var result = await CreateService().SubmitAnswerAsync(
            _playerId, Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitAnswerAsync_CorrectAnswer_IncreasesScoreAndReturnsNext()
    {
        var session = new GameSession(_playerId);
        var questionId = Guid.CreateVersion7();
        session.IssueQuestion(questionId); // this is the active question for the session
        _repo.Setup(r => r.GetForPlayerAsync(It.IsAny<Guid>(), _playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _quiz.Setup(q => q.Grade(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Result.Ok(new GradeResult(true, Guid.CreateVersion7())));
        QuizReturnsQuestion();

        var result = await CreateService().SubmitAnswerAsync(
            _playerId, session.Id, questionId, Guid.CreateVersion7());

        result.IsSuccess.Should().BeTrue();
        result.Value.Correct.Should().BeTrue();
        result.Value.Score.Should().Be(50);
        result.Value.NextQuestion.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitAnswerAsync_ForQuestionThatIsNotTheActiveOne_IsRejected()
    {
        // Replay / farming guard: the session's current question is A, but the client
        // submits some other id B. It must be refused before any grading happens.
        var session = new GameSession(_playerId);
        session.IssueQuestion(Guid.CreateVersion7());
        _repo.Setup(r => r.GetForPlayerAsync(It.IsAny<Guid>(), _playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await CreateService().SubmitAnswerAsync(
            _playerId, session.Id, Guid.CreateVersion7(), Guid.CreateVersion7());

        result.IsFailed.Should().BeTrue();
        _quiz.Verify(q => q.Grade(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never); // never graded
        _repo.Verify(r => r.UpdateAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitAnswerAsync_OnFinishedGame_ReturnsConflict()
    {
        var session = new GameSession(_playerId);
        session.Complete();
        _repo.Setup(r => r.GetForPlayerAsync(It.IsAny<Guid>(), _playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await CreateService().SubmitAnswerAsync(
            _playerId, session.Id, Guid.CreateVersion7(), Guid.CreateVersion7());

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task FinishAsync_CompletesAndReturnsSummary()
    {
        var session = new GameSession(_playerId);
        session.RecordAnswer(true);
        _repo.Setup(r => r.GetForPlayerAsync(It.IsAny<Guid>(), _playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await CreateService().FinishAsync(_playerId, session.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Outcome.Should().Be(GameOutcome.Won.ToString());
        result.Value.Score.Should().Be(50);
    }
}
