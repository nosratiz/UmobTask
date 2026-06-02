using FluentResults;
using GbfsQuiz.Application.Common.Errors;
using GbfsQuiz.Application.Features.Games.Interfaces;
using GbfsQuiz.Application.Features.Games.Responses;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;
using GbfsQuiz.Application.Features.Quiz.Responses;
using GbfsQuiz.Domain.Games;

namespace GbfsQuiz.Application.Features.Games;

/// <summary>Drives the 60-second game loop and enforces session ownership and timing.</summary>
public sealed class GameService(IGameSessionRepository repository, IQuizService quiz) : IGameService
{
    public async Task<Result<StartedGameResponse>> StartAsync(Guid playerId, CancellationToken ct = default)
    {
        var question = await quiz.NextQuestionAsync(ct);
        if (question.IsFailed)
        {
            return question.ToResult();
        }

        var session = new GameSession(playerId);
        await repository.CreateAsync(session, ct);

        var endsAt = session.StartedAtUtc.AddSeconds(GameSession.SessionDurationSeconds);
        return Result.Ok(new StartedGameResponse(
            session.Id, session.Score, endsAt, question.Value.ToResponse()));
    }

    public async Task<Result<AnswerResultResponse>> SubmitAnswerAsync(
        Guid playerId, Guid gameId, Guid questionId, Guid choiceId, CancellationToken ct = default)
    {
        var session = await repository.GetForPlayerAsync(gameId, playerId, ct);
        if (session is null)
        {
            return Result.Fail<AnswerResultResponse>(new NotFoundError($"Game '{gameId}' was not found."));
        }

        if (session.Outcome != GameOutcome.InProgress)
        {
            return Result.Fail<AnswerResultResponse>(new ConflictError("This game has already ended."));
        }

        return IsExpired(session)
            ? await ExpireAsync(session, ct)
            : await GradeAsync(session, questionId, choiceId, ct);
    }

    public async Task<Result<GameSummaryResponse>> FinishAsync(
        Guid playerId, Guid gameId, CancellationToken ct = default)
    {
        var session = await repository.GetForPlayerAsync(gameId, playerId, ct);
        if (session is null)
        {
            return Result.Fail<GameSummaryResponse>(new NotFoundError($"Game '{gameId}' was not found."));
        }

        if (session.Outcome == GameOutcome.InProgress)
        {
            session.Complete();
            await repository.UpdateAsync(session, ct);
        }

        return Result.Ok(session.ToSummary());
    }

    public async Task<Result<IReadOnlyList<GameSummaryResponse>>> GetHistoryAsync(
        Guid playerId, CancellationToken ct = default)
    {
        var sessions = await repository.GetHistoryAsync(playerId, ct);
        IReadOnlyList<GameSummaryResponse> summaries = sessions.Select(s => s.ToSummary()).ToList();
        return Result.Ok(summaries);
    }

    private async Task<Result<AnswerResultResponse>> GradeAsync(
        GameSession session, Guid questionId, Guid choiceId, CancellationToken ct)
    {
        var grade = quiz.Grade(questionId, choiceId);
        if (grade.IsFailed)
        {
            return grade.ToResult();
        }

        session.RecordAnswer(grade.Value.Correct);
        if (IsExpired(session))
        {
            session.Complete();
        }

        await repository.UpdateAsync(session, ct);
        return Result.Ok(await BuildResultAsync(session, grade.Value, ct));
    }

    private async Task<Result<AnswerResultResponse>> ExpireAsync(GameSession session, CancellationToken ct)
    {
        session.Complete();
        await repository.UpdateAsync(session, ct);
        return Result.Ok(new AnswerResultResponse(
            false, CorrectChoiceId: null, session.Score, 0, true, session.Outcome.ToString(), NextQuestion: null));
    }

    private async Task<AnswerResultResponse> BuildResultAsync(
        GameSession session, GradeResult grade, CancellationToken ct)
    {
        var next = await NextQuestionOrNullAsync(session, ct);
        return new AnswerResultResponse(
            grade.Correct,
            grade.CorrectChoiceId,
            session.Score,
            SecondsRemaining(session),
            session.Outcome != GameOutcome.InProgress,
            session.Outcome.ToString(),
            next);
    }

    private async Task<QuestionResponse?> NextQuestionOrNullAsync(GameSession session, CancellationToken ct)
    {
        if (session.Outcome != GameOutcome.InProgress)
        {
            return null;
        }

        var question = await quiz.NextQuestionAsync(ct);
        return question.IsSuccess ? question.Value.ToResponse() : null;
    }

    private static bool IsExpired(GameSession session) =>
        DateTimeOffset.UtcNow >= session.StartedAtUtc.AddSeconds(GameSession.SessionDurationSeconds);

    private static int SecondsRemaining(GameSession session)
    {
        var endsAt = session.StartedAtUtc.AddSeconds(GameSession.SessionDurationSeconds);
        var remaining = (endsAt - DateTimeOffset.UtcNow).TotalSeconds;
        return remaining <= 0 ? 0 : (int)Math.Ceiling(remaining);
    }
}
