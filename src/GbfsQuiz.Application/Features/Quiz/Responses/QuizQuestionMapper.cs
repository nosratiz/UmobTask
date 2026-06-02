using GbfsQuiz.Application.Features.Quiz.Models;

namespace GbfsQuiz.Application.Features.Quiz.Responses;

/// <summary>Projects the internal question (which carries the answer) to a safe client DTO.</summary>
public static class QuizQuestionMapper
{
    public static QuestionResponse ToResponse(this QuizQuestion question) =>
        new(question.Id,
            question.Category,
            question.Text,
            question.Choices.Select(c => new ChoiceResponse(c.Id, c.Text)).ToList());
}
