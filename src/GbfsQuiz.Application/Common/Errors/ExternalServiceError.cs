using FluentResults;

namespace GbfsQuiz.Application.Common.Errors;

/// <summary>Maps to HTTP 503. Used when an upstream dependency (a GBFS feed) fails.</summary>
public sealed class ExternalServiceError(string message) : Error(message)
{
}
