using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Interfaces;

/// <summary>
/// Short-lived server-side record of issued questions and their correct answer, so the
/// client never sees (and cannot tamper with) the right choice. Backed by a cache.
/// </summary>
public interface IIssuedQuestionStore
{
    void Remember(QuizQuestion question);

    bool TryGetCorrectChoice(Guid questionId, out Guid correctChoiceId);
}
