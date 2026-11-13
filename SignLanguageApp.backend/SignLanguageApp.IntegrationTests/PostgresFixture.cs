using Microsoft.EntityFrameworkCore;
using SignLanguageApp.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SignLanguageApp.IntegrationTests;

/// <summary>
/// Levanta un PostgreSQL 17 en Docker una sola vez para la colección y le aplica las
/// migraciones reales de Infrastructure.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    /// <summary>Cada llamada crea un DbContext nuevo, como pasa entre peticiones HTTP.</summary>
    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_container.GetConnectionString()).Options);

    /// <summary>Deja las tablas vacías para que cada prueba empiece de cero.</summary>
    public async Task ResetAsync()
    {
        await using var db = CreateContext();
        await db.Database.ExecuteSqlRawAsync(
            """TRUNCATE "TranslationRecords", "Consultations", "GeometricReferences", "Signs", "Categories", "Users" RESTART IDENTITY CASCADE""");
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
