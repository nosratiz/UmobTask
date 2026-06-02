using FluentResults;
using GbfsQuiz.Application.Common.Geo;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Strategies;

/// <summary>"Which station is geographically closest to {anchor station}?"</summary>
public sealed class NearestStationStrategy : IQuestionStrategy
{
    private const int CandidateCount = 3;

    public string Category => "NearestStation";

    public bool CanGenerate(IReadOnlyList<GbfsSnapshot> snapshots) =>
        snapshots.Any(s => s.Stations.Count >= CandidateCount + 1);

    public Result<QuizQuestion> Generate(IReadOnlyList<GbfsSnapshot> snapshots, Random rng)
    {
        var eligible = snapshots.Where(s => s.Stations.Count >= CandidateCount + 1).ToList();
        var snapshot = eligible[rng.Next(eligible.Count)];
        var pool = snapshot.Stations.OrderBy(_ => rng.Next()).Take(CandidateCount + 1).ToList();
        var anchor = pool[0];
        var candidates = pool.Skip(1).ToList();

        var nearest = candidates.MinBy(c => DistanceTo(anchor, c))!;
        var distractors = candidates.Where(c => c.Id != nearest.Id).Select(c => c.Name);

        return Result.Ok(QuestionFactory.Create(
            Category, $"Which station is geographically closest to \"{anchor.Name}\"?",
            nearest.Name, distractors, rng));
    }

    private static double DistanceTo(GbfsStation from, GbfsStation to) =>
        GeoCalculator.DistanceKm(from.Latitude, from.Longitude, to.Latitude, to.Longitude);
}
