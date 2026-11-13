using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.UseCases.Catalog;
using SignLanguageApp.Application.UseCases.Consultations;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.ValueObjects;
using SignLanguageApp.Infrastructure.Persistence;
using SignLanguageApp.Infrastructure.Persistence.Repositories;
using SignLanguageApp.Infrastructure.Security;

namespace SignLanguageApp.IntegrationTests.Persistence;

/// <summary>Casos de uso reales + repositorios reales + PostgreSQL real.</summary>
[Collection(PostgresCollection.Name)]
public sealed class SignLinkPersistenceTests(PostgresFixture postgres) : IAsyncLifetime
{
    private Guid _adminId;
    private Guid _doctorId;
    private int _categoryId;

    public async Task InitializeAsync()
    {
        await postgres.ResetAsync();
        await using var db = postgres.CreateContext();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seed:Admin:Email"] = "admin@test.local", ["Seed:Admin:Password"] = "Admin123!",
            ["Seed:Doctor:Email"] = "medico@test.local", ["Seed:Doctor:Password"] = "Medico123!",
        }).Build();
        await new DevelopmentSeeder(db, new PasswordHasherAdapter(), config, TimeProvider.System).SeedAsync();

        _adminId = (await db.Users.SingleAsync(u => u.Role == UserRole.Administrator)).Id;
        _doctorId = (await db.Users.SingleAsync(u => u.Role == UserRole.MedicalStaff)).Id;
        _categoryId = (await db.Categories.SingleAsync(c => c.Name == "Síntomas")).Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public void Migrations_match_the_model()
    {
        using var db = postgres.CreateContext();
        Assert.False(db.Database.HasPendingModelChanges(),
            "El modelo de EF cambió y falta una migración: dotnet ef migrations add <Nombre>");
    }

    [Fact]
    public async Task Seeder_is_idempotent_and_hashes_passwords()
    {
        await using var db = postgres.CreateContext();
        Assert.Equal(DevelopmentSeeder.Categories.Length, await db.Categories.CountAsync());

        var doctor = await db.Users.SingleAsync(u => u.Id == _doctorId);
        Assert.DoesNotContain("Medico123!", doctor.PasswordHash);
        Assert.True(new PasswordHasherAdapter().Verify(doctor, "Medico123!"));
    }

    [Fact]
    public async Task Demo_signs_are_seeded_only_when_enabled_and_catalog_is_empty()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:DemoSigns"] = "true" }).Build();

        await using (var db = postgres.CreateContext())
            await new DevelopmentSeeder(db, new PasswordHasherAdapter(), config, TimeProvider.System).SeedAsync();
        await using (var db = postgres.CreateContext())
            await new DevelopmentSeeder(db, new PasswordHasherAdapter(), config, TimeProvider.System).SeedAsync(); // idempotente

        await using var check = postgres.CreateContext();
        var signs = await new GetCatalogUseCase(new CatalogRepository(check)).GetSignsAsync(null);
        Assert.Equal(["ayuda", "bien", "dolor", "llamar", "no", "sí"], signs.Select(s => s.Word).ToArray());
        Assert.All(signs, s => Assert.Equal(DevelopmentSeeder.DemoCategory, s.CategoryName));
        Assert.All(signs, s => Assert.Equal(HandGeometry.AnglesPerHand, s.References.Single().Angles.Length));
    }

    [Fact]
    public async Task Registered_reference_round_trips_as_jsonb_and_filters_by_category()
    {
        var distances = Enumerable.Range(0, HandGeometry.DistancesPerHand).Select(i => i * 0.1).ToArray();
        await RegisterAsync("dolor", distances);

        await using var db = postgres.CreateContext();
        var catalog = new GetCatalogUseCase(new CatalogRepository(db));
        var signs = await catalog.GetSignsAsync(_categoryId);
        var other = await catalog.GetSignsAsync(_categoryId + 1);

        var sign = Assert.Single(signs);
        Assert.Equal("Síntomas", sign.CategoryName);
        Assert.Equal(distances, sign.References.Single().Distances);
        Assert.Empty(other);

        var columnType = await db.Database.SqlQueryRaw<string>(
            """SELECT data_type AS "Value" FROM information_schema.columns WHERE table_name = 'GeometricReferences' AND column_name = 'Distances'""")
            .SingleAsync();
        Assert.Equal("jsonb", columnType);
    }

    [Fact]
    public async Task Consultation_records_persist_across_contexts_and_clear_deletes_them()
    {
        var sign = await RegisterAsync("ayuda");
        var consultationId = await InNewContext(async (db, uc) => (await uc.Start.ExecuteAsync(_doctorId)).Id);

        await InNewContext((db, uc) => uc.Record.ExecuteAsync(consultationId, _doctorId, new RecordTranslationCommand(sign.Id, 91, null)));
        var pending = await InNewContext((db, uc) => uc.Record.ExecuteAsync(consultationId, _doctorId, new RecordTranslationCommand(sign.Id, 70, null)));
        await InNewContext((db, uc) => uc.Record.ExecuteAsync(consultationId, _doctorId, new RecordTranslationCommand(null, 20, null)));
        await InNewContext((db, uc) => uc.Review.ConfirmAsync(consultationId, pending.Id, _doctorId));

        var loaded = await InNewContext((db, uc) => uc.History.GetAsync(consultationId, _doctorId));
        Assert.Equal(
            [RecordStatus.Accepted, RecordStatus.Confirmed, RecordStatus.NotRecognized],
            loaded.Records.Select(r => r.Status).ToArray());

        await InNewContext((db, uc) => uc.History.ClearAsync(consultationId, _doctorId));
        await using var check = postgres.CreateContext();
        Assert.Equal(0, await check.TranslationRecords.CountAsync());
        Assert.Equal(1, await check.Consultations.CountAsync());
    }

    [Fact]
    public async Task Finish_persists_end_time_and_not_confirmed_words()
    {
        var sign = await RegisterAsync("fiebre");
        var id = await InNewContext(async (db, uc) => (await uc.Start.ExecuteAsync(_doctorId)).Id);
        await InNewContext((db, uc) => uc.Record.ExecuteAsync(id, _doctorId, new RecordTranslationCommand(sign.Id, 65, null)));
        await InNewContext((db, uc) => uc.Finish.ExecuteAsync(id, _doctorId));

        await using var db = postgres.CreateContext();
        var c = await db.Consultations.Include(x => x.Records).SingleAsync();
        Assert.Equal(ConsultationStatus.Finished, c.Status);
        Assert.NotNull(c.EndedAt);
        Assert.Equal(RecordStatus.NotConfirmed, c.Records.Single().Status);
    }

    [Fact]
    public async Task Database_check_constraint_rejects_fewer_than_five_samples()
    {
        var sign = await RegisterAsync("hospital");
        await using var db = postgres.CreateContext();

        var ex = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "GeometricReferences" ("Id","SignId","Distances","Angles","NormalizedLandmarks","SampleCount","CapturedAt","CapturedById")
            VALUES (gen_random_uuid(), {0}, '[]', '[]', '[]', 3, now(), {1})
            """, sign.Id, _adminId));
        Assert.Equal("23514", ex.SqlState); // check_violation
    }

    // ---------- helpers ----------

    private async Task<SignDto> RegisterAsync(string word, double[]? distances = null)
    {
        await using var db = postgres.CreateContext();
        var register = new RegisterSignReferenceUseCase(new CatalogRepository(db), db, TimeProvider.System);
        return await register.ExecuteAsync(new RegisterReferenceCommand(
            word, _categoryId, 1, null,
            distances ?? Enumerable.Repeat(0.5, HandGeometry.DistancesPerHand).ToArray(),
            Enumerable.Repeat(90.0, HandGeometry.AnglesPerHand).ToArray(),
            Enumerable.Repeat(0.1, HandGeometry.CoordinatesPerHand).ToArray(), 5), _adminId);
    }

    private sealed record UseCases(
        StartConsultationUseCase Start, RecordTranslationUseCase Record, ReviewTranslationUseCase Review,
        ConsultationHistoryUseCase History, FinishConsultationUseCase Finish);

    /// <summary>Ejecuta con un DbContext nuevo, igual que en la API (un scope por petición).</summary>
    private async Task<T> InNewContext<T>(Func<AppDbContext, UseCases, Task<T>> action)
    {
        await using var db = postgres.CreateContext();
        var consultations = new ConsultationRepository(db);
        var catalog = new CatalogRepository(db);
        var clock = TimeProvider.System;
        var uc = new UseCases(
            new StartConsultationUseCase(consultations, db, clock),
            new RecordTranslationUseCase(consultations, catalog, db, Options.Create(new CertaintyOptions()), clock),
            new ReviewTranslationUseCase(consultations, db),
            new ConsultationHistoryUseCase(consultations, db),
            new FinishConsultationUseCase(consultations, db, clock));
        return await action(db, uc);
    }
}
