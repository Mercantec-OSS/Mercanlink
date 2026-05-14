using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Backend.Tests;

/// <summary>
/// Verificerer at EF-migrationer kan anvendes mod en rigtig Postgres (Testcontainers),
/// uden at starte hele web-appen (Discord, hosted jobs, osv.).
/// </summary>
public sealed class PostgresMigrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .Build();
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }

    [Fact]
    public async Task Migrations_apply_and_database_connects()
    {
        Assert.NotNull(_postgres);
        var connectionString = _postgres.GetConnectionString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var db = new ApplicationDbContext(options);
        await db.Database.MigrateAsync();
        Assert.True(await db.Database.CanConnectAsync());
    }
}
