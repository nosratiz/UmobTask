using GbfsQuiz.Domain.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GbfsQuiz.Infrastructure.Persistence.Configurations;

/// <summary>Fluent configuration for <see cref="Player"/> with a UUIDv7 key and unique username.</summary>
public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Username).HasColumnName("username").HasMaxLength(32).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(64).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.AvatarPath).HasColumnName("avatar_path").HasMaxLength(256);

        builder.HasIndex(x => x.Username).IsUnique();
    }
}
