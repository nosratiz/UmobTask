using GbfsQuiz.Domain.Common;

namespace GbfsQuiz.Domain.Games;

/// <summary>
/// A single 60-second quiz attempt. Encapsulates scoring rules
/// (+50 correct / -20 wrong) and the win condition (score must remain
/// strictly positive for the whole session).
/// </summary>
public sealed class GameSession : Entity
{
    public const int SessionDurationSeconds = 60;
    public const int CorrectAnswerPoints = 50;
    public const int WrongAnswerPenalty = 20;

    private GameSession() { } // EF Core materialization.

    public GameSession(Guid playerId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(playerId, Guid.Empty);

        PlayerId = playerId;
        StartedAtUtc = DateTimeOffset.UtcNow;
        Outcome = GameOutcome.InProgress;
    }

    public Guid PlayerId { get; private set; }
    public int Score { get; private set; }
    public int CorrectAnswers { get; private set; }
    public int WrongAnswers { get; private set; }
    public bool StayedPositive { get; private set; } = true;
    public GameOutcome Outcome { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }

    /// <summary>
    /// The id of the question the player may currently answer. Only this id is accepted by
    /// <see cref="RecordAnswer"/>, so a client cannot replay an already-answered question
    /// (or submit a foreign one) to farm points.
    /// </summary>
    public Guid? CurrentQuestionId { get; private set; }

    /// <summary>Flexible, provider-specific context persisted as PostgreSQL JSONB.</summary>
    public Dictionary<string, string> Metadata { get; private set; } = new();

    /// <summary>Marks <paramref name="questionId"/> as the one outstanding answerable question.</summary>
    public void IssueQuestion(Guid questionId)
    {
        ThrowIfNotInProgress();
        ArgumentOutOfRangeException.ThrowIfEqual(questionId, Guid.Empty);
        CurrentQuestionId = questionId;
    }

    /// <summary>True when <paramref name="questionId"/> is the session's current outstanding question.</summary>
    public bool IsCurrentQuestion(Guid questionId) =>
        CurrentQuestionId is { } current && current == questionId;

    public void RecordAnswer(bool isCorrect)
    {
        ThrowIfNotInProgress();

        // Consume the outstanding question so the same id can never be scored twice.
        CurrentQuestionId = null;

        if (isCorrect)
        {
            Score += CorrectAnswerPoints;
            CorrectAnswers++;
            return;
        }

        Score -= WrongAnswerPenalty;
        WrongAnswers++;
        if (Score <= 0)
        {
            StayedPositive = false;
        }
    }

    public void Complete()
    {
        ThrowIfNotInProgress();
        EndedAtUtc = DateTimeOffset.UtcNow;
        Outcome = StayedPositive && Score > 0 ? GameOutcome.Won : GameOutcome.Lost;
    }

    private void ThrowIfNotInProgress()
    {
        if (Outcome != GameOutcome.InProgress)
        {
            throw new InvalidOperationException("The game session has already ended.");
        }
    }
}
