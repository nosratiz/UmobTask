using FluentAssertions;
using GbfsQuiz.Infrastructure.Persistence;
using Xunit;

namespace GbfsQuiz.Tests.Infrastructure;

public sealed class PostgresConnectionStringTests
{
    [Fact]
    public void Normalize_ConvertsPostgresUriToNpgsqlKeyValueForm()
    {
        var result = PostgresConnectionString.Normalize(
            "postgres://gbfs:secret@db.host:5432/gbfs_quiz?sslmode=require");

        result.Should().Contain("Host=db.host")
            .And.Contain("Port=5432")
            .And.Contain("Username=gbfs")
            .And.Contain("Password=secret")
            .And.Contain("Database=gbfs_quiz")
            .And.Contain("SSL Mode=Require");
    }

    [Fact]
    public void Normalize_LeavesKeyValueConnectionStringUnchanged()
    {
        const string keyValue = "Host=localhost;Database=gbfs_quiz;Username=postgres;Password=postgres";

        PostgresConnectionString.Normalize(keyValue).Should().Be(keyValue);
    }
}
