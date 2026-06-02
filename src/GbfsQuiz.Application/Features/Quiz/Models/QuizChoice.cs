namespace GbfsQuiz.Application.Features.Quiz.Models;

/// <summary>One selectable answer in a multiple-choice question.</summary>
public sealed record QuizChoice(Guid Id, string Text);
