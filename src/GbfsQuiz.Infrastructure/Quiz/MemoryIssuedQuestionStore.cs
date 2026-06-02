using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Models;
using Microsoft.Extensions.Caching.Memory;

namespace GbfsQuiz.Infrastructure.Quiz;

/// <summary>
/// Remembers a question's correct answer for a couple of minutes — long enough to grade
/// it within a 60-second game, short enough to self-evict. Grading is single-use: the
/// answer is removed the first time it is consumed, so a question cannot be replayed.
/// </summary>
public sealed class MemoryIssuedQuestionStore(IMemoryCache cache) : IIssuedQuestionStore
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(2);

    // IMemoryCache has no atomic get-and-remove, so a lock makes consume-once race-free:
    // two concurrent submissions of the same question can never both succeed.
    private readonly Lock _gate = new();

    public void Remember(QuizQuestion question)
    {
        lock (_gate)
        {
            cache.Set(Key(question.Id), question.CorrectChoiceId, Ttl);
        }
    }

    public bool TryConsumeCorrectChoice(Guid questionId, out Guid correctChoiceId)
    {
        lock (_gate)
        {
            var key = Key(questionId);
            if (cache.TryGetValue(key, out correctChoiceId))
            {
                cache.Remove(key);
                return true;
            }

            return false;
        }
    }

    private static string Key(Guid questionId) => $"quiz:q:{questionId}";
}
