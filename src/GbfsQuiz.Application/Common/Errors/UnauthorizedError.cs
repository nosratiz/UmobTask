using FluentResults;

namespace GbfsQuiz.Application.Common.Errors;

/// <summary>Maps to HTTP 401. Used when authentication is required or invalid.</summary>
public sealed class UnauthorizedError(string message) : Error(message)
{
}
