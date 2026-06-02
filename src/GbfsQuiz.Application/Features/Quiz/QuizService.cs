using FluentResults;
using GbfsQuiz.Application.Common.Errors;
using GbfsQuiz.Application.Features.Gbfs.Interfaces;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz;

/// <summary>
/// Picks a random eligible question strategy for the current snapshots, generates a
/// question, and remembers its answer for later grading.
/// </summary>
public sealed class QuizService(
    IGbfsSnapshotProvider snapshots,
    IEnumerable<IQuestionStrategy> strategies,
    IIssuedQuestionStore store) : IQuizService
{
    private readonly IReadOnlyList<IQuestionStrategy> _strategies = strategies.ToList();

    public async Task<Result<QuizQuestion>> NextQuestionAsync(CancellationToken ct = default)
    {
        var snapshotResult = await snapshots.GetAllAsync(ct);
        if (snapshotResult.IsFailed)
        {
            return snapshotResult.ToResult();
        }

        return BuildQuestion(snapshotResult.Value);
    }

    public Result<GradeResult> Grade(Guid questionId, Guid choiceId) =>
        store.TryConsumeCorrectChoice(questionId, out var correct)
            ? Result.Ok(new GradeResult(choiceId == correct, correct))
            : Result.Fail<GradeResult>(new NotFoundError("That question is no longer active."));

    private Result<QuizQuestion> BuildQuestion(IReadOnlyList<GbfsSnapshot> data)
    {
        var eligible = _strategies.Where(s => s.CanGenerate(data)).ToList();
        if (eligible.Count == 0)
        {
            return Result.Fail<QuizQuestion>(
                new ExternalServiceError("Not enough live GBFS data to build a question."));
        }

        var strategy = eligible[Random.Shared.Next(eligible.Count)];
        var question = strategy.Generate(data, Random.Shared);
        if (question.IsSuccess)
        {
            store.Remember(question.Value);
        }

        return question;
    }
}
