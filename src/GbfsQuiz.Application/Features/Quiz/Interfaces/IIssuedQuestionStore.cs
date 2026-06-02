using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Interfaces;

/// <summary>
/// Short-lived server-side record of issued questions and their correct answer, so the
/// client never sees (and cannot tamper with) the right choice. Backed by a cache.
/// </summary>
public interface IIssuedQuestionStore
{
    void Remember(QuizQuestion question);

    /// <summary>
    /// Atomically looks up and <b>removes</b> the correct choice for a question. A question
    /// can be graded exactly once: a second (or concurrent) call returns <c>false</c>. This
    /// is what prevents answer-replay / score farming.
    /// </summary>
    bool TryConsumeCorrectChoice(Guid questionId, out Guid correctChoiceId);
}
