using FluentResults;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Strategies;

/// <summary>"Which city currently has the most available bikes?"</summary>
public sealed class MostBikesCityStrategy : IQuestionStrategy
{
    public string Category => "MostAvailableBikes";

    public bool CanGenerate(IReadOnlyList<GbfsSnapshot> snapshots) => snapshots.Count >= 2;

    public Result<QuizQuestion> Generate(IReadOnlyList<GbfsSnapshot> snapshots, Random rng)
    {
        var ranked = snapshots.OrderByDescending(s => s.TotalBikesAvailable).ToList();
        var winner = ranked[0].Provider.City;
        var distractors = ranked.Skip(1).Select(s => s.Provider.City);

        return Result.Ok(QuestionFactory.Create(
            Category, "Which city currently has the most available bikes?", winner, distractors, rng));
    }
}
