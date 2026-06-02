using GbfsQuiz.Application.Features.Quiz.Responses;

namespace GbfsQuiz.Application.Features.Games.Responses;

/// <summary>Returned when a new 60-second session starts, with its first question.</summary>
public sealed record StartedGameResponse(
    Guid GameId,
    int Score,
    DateTimeOffset EndsAtUtc,
    QuestionResponse Question);
