using FluentResults;

namespace GbfsQuiz.Application.Common.Errors;

/// <summary>Maps to HTTP 409. Used when an operation conflicts with current state.</summary>
public sealed class ConflictError(string message) : Error(message)
{
}
