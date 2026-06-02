using System.Text.Json;
using GbfsQuiz.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GbfsQuiz.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent configuration for <see cref="GameSession"/>. The primary key is a
/// client-generated UUIDv7 (no DB default), and free-form context is stored
/// in a PostgreSQL JSONB column.
/// </summary>
public sealed class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("game_sessions");
        builder.HasKey(x => x.Id);

        // The application supplies the UUIDv7; EF must not generate or override it.
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.PlayerId).HasColumnName("player_id").IsRequired();
        builder.Property(x => x.Score).HasColumnName("score").IsRequired();
        builder.Property(x => x.CorrectAnswers).HasColumnName("correct_answers").IsRequired();
        builder.Property(x => x.WrongAnswers).HasColumnName("wrong_answers").IsRequired();
        builder.Property(x => x.StayedPositive).HasColumnName("stayed_positive").IsRequired();
        builder.Property(x => x.Outcome).HasColumnName("outcome").HasConversion<string>().IsRequired();
        builder.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc").IsRequired();
        builder.Property(x => x.EndedAtUtc).HasColumnName("ended_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        ConfigureJsonbMetadata(builder);

        builder.HasIndex(x => x.PlayerId);
        builder.HasIndex(x => new { x.PlayerId, x.CreatedAtUtc });
    }

    private static void ConfigureJsonbMetadata(EntityTypeBuilder<GameSession> builder)
    {
        var comparer = new ValueComparer<Dictionary<string, string>>(
            (l, r) => JsonSerializer.Serialize(l, JsonOptions) == JsonSerializer.Serialize(r, JsonOptions),
            v => v == null ? 0 : JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(
                JsonSerializer.Serialize(v, JsonOptions), JsonOptions)!);

        builder.Property(x => x.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions) ?? new())
            .Metadata.SetValueComparer(comparer);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
