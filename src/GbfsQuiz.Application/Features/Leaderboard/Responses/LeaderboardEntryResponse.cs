namespace GbfsQuiz.Application.Features.Leaderboard.Responses;

/// <summary>A ranked row in the public leaderboard.</summary>
public sealed record LeaderboardEntryResponse(
    int Rank,
    Guid PlayerId,
    string DisplayName,
    bool HasAvatar,
    int BestScore,
    int GamesWon,
    int GamesPlayed);
