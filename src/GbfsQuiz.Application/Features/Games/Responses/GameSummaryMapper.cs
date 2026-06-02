using GbfsQuiz.Domain.Games;

namespace GbfsQuiz.Application.Features.Games.Responses;

/// <summary>Projects a persisted <see cref="GameSession"/> to its history DTO.</summary>
public static class GameSummaryMapper
{
    public static GameSummaryResponse ToSummary(this GameSession session) =>
        new(session.Id,
            session.Score,
            session.Outcome.ToString(),
            session.CorrectAnswers,
            session.WrongAnswers,
            session.StartedAtUtc,
            session.EndedAtUtc);
}
