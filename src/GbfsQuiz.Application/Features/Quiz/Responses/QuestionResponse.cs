namespace GbfsQuiz.Application.Features.Quiz.Responses;

/// <summary>A question as exposed to the client — deliberately omits the correct choice.</summary>
public sealed record QuestionResponse(
    Guid Id,
    string Category,
    string Text,
    IReadOnlyList<ChoiceResponse> Choices);
