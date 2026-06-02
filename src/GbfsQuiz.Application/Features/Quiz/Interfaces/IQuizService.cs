using FluentResults;
using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Interfaces;

/// <summary>Generates questions from live GBFS data and grades submitted answers.</summary>
public interface IQuizService
{
    Task<Result<QuizQuestion>> NextQuestionAsync(CancellationToken ct = default);

    Result<GradeResult> Grade(Guid questionId, Guid choiceId);
}
