using FluentResults;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Strategies;

/// <summary>"How many docking stations does {City}'s network have?"</summary>
public sealed class StationCountStrategy : IQuestionStrategy
{
    public string Category => "StationCount";

    public bool CanGenerate(IReadOnlyList<GbfsSnapshot> snapshots) =>
        snapshots.Any(s => s.StationCount > 5);

    public Result<QuizQuestion> Generate(IReadOnlyList<GbfsSnapshot> snapshots, Random rng)
    {
        var eligible = snapshots.Where(s => s.StationCount > 5).ToList();
        var snapshot = eligible[rng.Next(eligible.Count)];
        var correct = snapshot.StationCount;
        var distractors = NumericDistractorGenerator.Generate(correct, 3, rng);

        return Result.Ok(QuestionFactory.Create(
            Category,
            $"How many docking stations does the {snapshot.Provider.Name} network in {snapshot.Provider.City} have?",
            correct.ToString(), distractors, rng));
    }
}
