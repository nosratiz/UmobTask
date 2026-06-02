namespace GbfsQuiz.Application.Features.Quiz.Models;

/// <summary>
/// Outcome of grading an answer: whether it was correct and which choice was right.
/// The correct choice is only revealed after the player has answered.
/// </summary>
public sealed record GradeResult(bool Correct, Guid CorrectChoiceId);
