using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Strategies;

/// <summary>Assembles a question from a correct label plus distractors, with shuffled choices.</summary>
public static class QuestionFactory
{
    public static QuizQuestion Create(
        string category, string text, string correctLabel, IEnumerable<string> distractors, Random rng)
    {
        var correct = new QuizChoice(Guid.CreateVersion7(), correctLabel);
        var choices = distractors
            .Select(d => new QuizChoice(Guid.CreateVersion7(), d))
            .Append(correct)
            .OrderBy(_ => rng.Next())
            .ToList();

        return new QuizQuestion(Guid.CreateVersion7(), category, text, choices, correct.Id);
    }
}
