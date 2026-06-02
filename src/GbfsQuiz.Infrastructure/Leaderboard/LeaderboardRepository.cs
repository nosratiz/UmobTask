using GbfsQuiz.Application.Features.Leaderboard.Interfaces;
using GbfsQuiz.Application.Features.Leaderboard.Models;
using GbfsQuiz.Domain.Games;
using GbfsQuiz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GbfsQuiz.Infrastructure.Leaderboard;

/// <summary>
/// Aggregates completed sessions into per-player standings, ranked by best score then
/// wins. Owns a single context for its sequential queries (created via the factory, so
/// it never shares a DbContext with concurrently-rendering components). Counts are split
/// into separate translatable queries (filtering a value-converted enum inside a GROUP BY
/// aggregate is not provider-translatable).
/// </summary>
public sealed class LeaderboardRepository(IDbContextFactory<AppDbContext> contextFactory)
    : EfRepository(contextFactory), ILeaderboardRepository
{
    public Task<IReadOnlyList<LeaderboardStanding>> GetTopAsync(int limit, CancellationToken ct = default) =>
        QueryAsync(async (db, c) =>
        {
            var played = await QueryPlayedAsync(db, c);
            if (played.Count == 0)
            {
                return (IReadOnlyList<LeaderboardStanding>)[];
            }

            var wins = await QueryWinsAsync(db, c);
            var ranked = played
                .Select(p => p with { GamesWon = wins.GetValueOrDefault(p.PlayerId) })
                .OrderByDescending(p => p.BestScore)
                .ThenByDescending(p => p.GamesWon)
                .Take(limit)
                .ToList();

            var players = await LoadPlayersAsync(db, ranked.Select(r => r.PlayerId).ToList(), c);
            return ranked
                .Where(r => players.ContainsKey(r.PlayerId))
                .Select(r => ToStanding(r, players[r.PlayerId]))
                .ToList();
        }, ct);

    private static async Task<List<Aggregate>> QueryPlayedAsync(AppDbContext db, CancellationToken ct) =>
        await db.GameSessions
            .AsNoTracking()
            .Where(s => s.Outcome != GameOutcome.InProgress)
            .GroupBy(s => s.PlayerId)
            .Select(g => new Aggregate(g.Key, g.Max(x => x.Score), 0, g.Count()))
            .ToListAsync(ct);

    private static async Task<Dictionary<Guid, int>> QueryWinsAsync(AppDbContext db, CancellationToken ct) =>
        await db.GameSessions
            .AsNoTracking()
            .Where(s => s.Outcome == GameOutcome.Won)
            .GroupBy(s => s.PlayerId)
            .Select(g => new { PlayerId = g.Key, Wins = g.Count() })
            .ToDictionaryAsync(x => x.PlayerId, x => x.Wins, ct);

    private static async Task<Dictionary<Guid, PlayerInfo>> LoadPlayersAsync(
        AppDbContext db, List<Guid> ids, CancellationToken ct) =>
        await db.Players
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new PlayerInfo(p.Id, p.DisplayName, p.AvatarPath != null))
            .ToDictionaryAsync(p => p.Id, ct);

    private static LeaderboardStanding ToStanding(Aggregate a, PlayerInfo player) =>
        new(a.PlayerId, player.DisplayName, player.HasAvatar, a.BestScore, a.GamesWon, a.GamesPlayed);

    private sealed record Aggregate(Guid PlayerId, int BestScore, int GamesWon, int GamesPlayed);

    private sealed record PlayerInfo(Guid Id, string DisplayName, bool HasAvatar);
}
