using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;
using Microsoft.Extensions.Caching.Memory;

namespace GbfsQuiz.Infrastructure.Quiz;

/// <summary>
/// Remembers a question's correct answer for a couple of minutes — long enough to grade
/// it within a 60-second game, short enough to self-evict.
/// </summary>
public sealed class MemoryIssuedQuestionStore(IMemoryCache cache) : IIssuedQuestionStore
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(2);

    public void Remember(QuizQuestion question) =>
        cache.Set(Key(question.Id), question.CorrectChoiceId, Ttl);

    public bool TryGetCorrectChoice(Guid questionId, out Guid correctChoiceId) =>
        cache.TryGetValue(Key(questionId), out correctChoiceId);

    private static string Key(Guid questionId) => $"quiz:q:{questionId}";
}
