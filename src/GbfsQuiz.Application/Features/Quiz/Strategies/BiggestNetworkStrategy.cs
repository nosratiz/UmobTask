using FluentResults;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Strategies;

/// <summary>"Which provider operates the most docking stations?"</summary>
public sealed class BiggestNetworkStrategy : IQuestionStrategy
{
    public string Category => "BiggestNetwork";

    public bool CanGenerate(IReadOnlyList<GbfsSnapshot> snapshots) => snapshots.Count >= 2;

    public Result<QuizQuestion> Generate(IReadOnlyList<GbfsSnapshot> snapshots, Random rng)
    {
        var ranked = snapshots.OrderByDescending(s => s.StationCount).ToList();
        var winner = Describe(ranked[0].Provider);
        var distractors = ranked.Skip(1).Select(s => Describe(s.Provider));

        return Result.Ok(QuestionFactory.Create(
            Category, "Which provider operates the most docking stations?", winner, distractors, rng));
    }

    private static string Describe(GbfsProvider provider) => $"{provider.Name} ({provider.City})";
}
