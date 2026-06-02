namespace GbfsQuiz.Domain.Games;

/// <summary>
/// Terminal state of a quiz session. A session is won only if the running
/// score stayed strictly positive for the full 60-second duration.
/// </summary>
public enum GameOutcome
{
    InProgress = 0,
    Won = 1,
    Lost = 2
}
