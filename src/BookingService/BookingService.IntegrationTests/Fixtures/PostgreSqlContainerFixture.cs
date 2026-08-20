using Testcontainers.PostgreSql;

namespace BookingService.IntegrationTests.Fixtures;

public class PostgreSqlContainerFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("testContainers")
        .Build();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
}