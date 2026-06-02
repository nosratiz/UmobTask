namespace GbfsQuiz.Application.Features.Games.Responses;

/// <summary>A historic attempt as shown in the player's history list.</summary>
public sealed record GameSummaryResponse(
    Guid GameId,
    int Score,
    string Outcome,
    int CorrectAnswers,
    int WrongAnswers,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? EndedAtUtc);
