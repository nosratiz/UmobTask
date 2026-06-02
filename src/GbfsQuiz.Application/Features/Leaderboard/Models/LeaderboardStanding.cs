namespace GbfsQuiz.Application.Features.Leaderboard.Models;

/// <summary>Aggregated, unranked performance for one player (built by the repository).</summary>
public sealed record LeaderboardStanding(
    Guid PlayerId,
    string DisplayName,
    bool HasAvatar,
    int BestScore,
    int GamesWon,
    int GamesPlayed);
