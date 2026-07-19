using EventOrchestrationService.Infrastructure.Data;
using EventOrchestrationService.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventOrchestrationService.IntegrationTests;

[Collection("Database collection")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer Postgres;
    private readonly DbContextOptions<AppDbContext> _options;

    protected IntegrationTestBase(PostgreSqlContainerFixture fixture)
    {
        Postgres = fixture.Container;

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(Postgres.GetConnectionString())
            .Options;
    }

    protected AppDbContext CreateContext()
    {
        var context = new AppDbContext(_options);
        return context;
    }

    private async Task ResetDatabaseAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Events\" RESTART IDENTITY CASCADE;");
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}