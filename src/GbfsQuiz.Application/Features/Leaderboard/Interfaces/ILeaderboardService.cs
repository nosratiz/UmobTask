using FluentResults;
using GbfsQuiz.Application.Features.Leaderboard.Responses;

namespace GbfsQuiz.Application.Features.Leaderboard.Interfaces;

/// <summary>Builds the ranked, public multiplayer leaderboard.</summary>
public interface ILeaderboardService
{
    Task<Result<IReadOnlyList<LeaderboardEntryResponse>>> GetTopAsync(int limit = 10, CancellationToken ct = default);
}
