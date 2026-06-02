using GbfsQuiz.Application.Features.Leaderboard.Models;

namespace GbfsQuiz.Application.Features.Leaderboard.Interfaces;

/// <summary>Reads aggregated player standings from completed game sessions.</summary>
public interface ILeaderboardRepository
{
    Task<IReadOnlyList<LeaderboardStanding>> GetTopAsync(int limit, CancellationToken ct = default);
}
