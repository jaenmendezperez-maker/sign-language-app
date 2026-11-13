using Microsoft.EntityFrameworkCore;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence;

/// <summary>Implementa el modelo ER de docs/modelo-er.png (equivalencia de nombres en el Anexo A del documento).</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Sign> Signs => Set<Sign>();
    public DbSet<GeometricReference> GeometricReferences => Set<GeometricReference>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<TranslationRecord> TranslationRecords => Set<TranslationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);
}
