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

    /// <summary>Flexible, provider-specific context persisted as PostgreSQL JSONB.</summary>
    public Dictionary<string, string> Metadata { get; private set; } = new();

    public void RecordAnswer(bool isCorrect)
    {
        ThrowIfNotInProgress();
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
