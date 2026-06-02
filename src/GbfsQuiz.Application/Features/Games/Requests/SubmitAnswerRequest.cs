namespace GbfsQuiz.Application.Features.Games.Requests;

/// <summary>
/// Player's answer submission. Deliberately narrow (white-listed) to prevent mass
/// assignment — the game id comes from the route, never the body.
/// </summary>
public sealed record SubmitAnswerRequest(Guid QuestionId, Guid ChoiceId);
