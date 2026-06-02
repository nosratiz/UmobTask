using FluentResults;
using GbfsQuiz.Application.Features.Leaderboard.Interfaces;
using GbfsQuiz.Application.Features.Leaderboard.Responses;

namespace GbfsQuiz.Application.Features.Leaderboard;

/// <summary>Ranks player standings into the public leaderboard (best score, then wins).</summary>
public sealed class LeaderboardService(ILeaderboardRepository repository) : ILeaderboardService
{
    private const int MaxLimit = 50;

    public async Task<Result<IReadOnlyList<LeaderboardEntryResponse>>> GetTopAsync(
        int limit = 10, CancellationToken ct = default)
    {
        var bounded = Math.Clamp(limit, 1, MaxLimit);
        var standings = await repository.GetTopAsync(bounded, ct);

        IReadOnlyList<LeaderboardEntryResponse> ranked = standings
            .Select((s, index) => new LeaderboardEntryResponse(
                index + 1, s.PlayerId, s.DisplayName, s.HasAvatar, s.BestScore, s.GamesWon, s.GamesPlayed))
            .ToList();

        return Result.Ok(ranked);
    }
}
