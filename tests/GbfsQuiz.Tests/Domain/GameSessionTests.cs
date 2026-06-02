using FluentAssertions;
using GbfsQuiz.Domain.Games;
using Xunit;

namespace GbfsQuiz.Tests.Domain;

public sealed class GameSessionTests
{
    private static GameSession NewSession() => new(Guid.CreateVersion7());

    [Fact]
    public void RecordAnswer_Correct_Adds50AndCountsIt()
    {
        var session = NewSession();

        session.RecordAnswer(isCorrect: true);

        session.Score.Should().Be(50);
        session.CorrectAnswers.Should().Be(1);
    }

    [Fact]
    public void RecordAnswer_Wrong_Subtracts20AndCountsIt()
    {
        var session = NewSession();

        session.RecordAnswer(isCorrect: true);
        session.RecordAnswer(isCorrect: false);

        session.Score.Should().Be(30);
        session.WrongAnswers.Should().Be(1);
    }

    [Fact]
    public void Complete_WithPositiveScoreThatNeverDropped_Wins()
    {
        var session = NewSession();
        session.RecordAnswer(true);
        session.RecordAnswer(true);

        session.Complete();

        session.Outcome.Should().Be(GameOutcome.Won);
    }

    [Fact]
    public void Complete_WhenScoreDippedToZeroOrBelow_Loses()
    {
        var session = NewSession();
        session.RecordAnswer(true);   // 50
        session.RecordAnswer(false);  // 30
        session.RecordAnswer(false);  // 10
        session.RecordAnswer(false);  // -10 -> dropped below zero

        session.Complete();

        session.Outcome.Should().Be(GameOutcome.Lost);
        session.StayedPositive.Should().BeFalse();
    }

    [Fact]
    public void Complete_WithNoAnswers_LosesBecauseBalanceNotPositive()
    {
        var session = NewSession();

        session.Complete();

        session.Outcome.Should().Be(GameOutcome.Lost);
    }

    [Fact]
    public void RecordAnswer_AfterCompletion_Throws()
    {
        var session = NewSession();
        session.Complete();

        var act = () => session.RecordAnswer(true);

        act.Should().Throw<InvalidOperationException>();
    }
}
