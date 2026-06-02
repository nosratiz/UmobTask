using FluentResults;
using GbfsQuiz.Application.Features.Gbfs.Models;
using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Interfaces;

/// <summary>
/// A single question type. Strategies are pure: given the current snapshots and a
/// random source, they decide whether they can produce a question and, if so, build one.
/// </summary>
public interface IQuestionStrategy
{
    string Category { get; }

    bool CanGenerate(IReadOnlyList<GbfsSnapshot> snapshots);

    Result<QuizQuestion> Generate(IReadOnlyList<GbfsSnapshot> snapshots, Random rng);
}
