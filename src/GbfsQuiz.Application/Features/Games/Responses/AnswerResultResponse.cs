using GbfsQuiz.Application.Features.Quiz.Responses;

namespace GbfsQuiz.Application.Features.Games.Responses;

/// <summary>
/// Result of grading one answer: whether it was correct, the running score, time left,
/// and the next question (null once the session is over).
/// </summary>
public sealed record AnswerResultResponse(
    bool Correct,
    Guid? CorrectChoiceId,
    int Score,
    int SecondsRemaining,
    bool GameOver,
    string Outcome,
    QuestionResponse? NextQuestion);
