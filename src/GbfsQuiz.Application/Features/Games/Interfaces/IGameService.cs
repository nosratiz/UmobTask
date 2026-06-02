using FluentResults;
using GbfsQuiz.Application.Features.Games.Responses;

namespace GbfsQuiz.Application.Features.Games.Interfaces;

/// <summary>Orchestrates the 60-second game loop: start, answer, finish, history.</summary>
public interface IGameService
{
    Task<Result<StartedGameResponse>> StartAsync(Guid playerId, CancellationToken ct = default);

    Task<Result<AnswerResultResponse>> SubmitAnswerAsync(
        Guid playerId, Guid gameId, Guid questionId, Guid choiceId, CancellationToken ct = default);

    Task<Result<GameSummaryResponse>> FinishAsync(Guid playerId, Guid gameId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<GameSummaryResponse>>> GetHistoryAsync(
        Guid playerId, CancellationToken ct = default);
}
