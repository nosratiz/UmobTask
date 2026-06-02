using FluentResults;

namespace GbfsQuiz.Application.Common.Errors;

/// <summary>Maps to HTTP 404. Used when a requested resource does not exist.</summary>
public sealed class NotFoundError(string message) : Error(message)
{
}
