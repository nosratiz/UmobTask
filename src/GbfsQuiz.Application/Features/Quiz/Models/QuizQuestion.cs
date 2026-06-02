namespace GbfsQuiz.Application.Features.Quiz.Models;

/// <summary>
/// A generated multiple-choice question. <see cref="CorrectChoiceId"/> is server-side
/// truth and must never be serialised to the player — endpoints map to a response DTO
/// that omits it.
/// </summary>
public sealed record QuizQuestion(
    Guid Id,
    string Category,
    string Text,
    IReadOnlyList<QuizChoice> Choices,
    Guid CorrectChoiceId);
