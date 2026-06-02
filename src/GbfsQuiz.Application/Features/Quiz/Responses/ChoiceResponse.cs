namespace GbfsQuiz.Application.Features.Quiz.Responses;

/// <summary>A selectable answer as exposed to the client (no correctness flag).</summary>
public sealed record ChoiceResponse(Guid Id, string Text);
