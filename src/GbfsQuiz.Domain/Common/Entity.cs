namespace GbfsQuiz.Domain.Common;

/// <summary>
/// Base type for all persisted aggregates. The identifier is a UUIDv7
/// (time-ordered GUID) generated on construction so primary keys remain
/// index-friendly under PostgreSQL without relying on DB-side defaults.
/// </summary>
public abstract class Entity
{
    protected Entity() => Id = Guid.CreateVersion7();

    public Guid Id { get; protected set; }

    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
}
