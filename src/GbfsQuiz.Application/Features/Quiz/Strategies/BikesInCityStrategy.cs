using FluentResults;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Strategies;

/// <summary>"Approximately how many bikes are available right now in {City}?"</summary>
public sealed class BikesInCityStrategy : IQuestionStrategy
{
    public string Category => "BikesInCity";

    public bool CanGenerate(IReadOnlyList<GbfsSnapshot> snapshots) =>
        snapshots.Any(s => s.TotalBikesAvailable > 5);

    public Result<QuizQuestion> Generate(IReadOnlyList<GbfsSnapshot> snapshots, Random rng)
    {
        var eligible = snapshots.Where(s => s.TotalBikesAvailable > 5).ToList();
        var snapshot = eligible[rng.Next(eligible.Count)];
        var correct = snapshot.TotalBikesAvailable;
        var distractors = NumericDistractorGenerator.Generate(correct, 3, rng);

        return Result.Ok(QuestionFactory.Create(
            Category,
            $"Approximately how many bikes are available right now in {snapshot.Provider.City}?",
            correct.ToString(), distractors, rng));
    }
}
