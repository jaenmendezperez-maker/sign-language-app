using Microsoft.EntityFrameworkCore;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TranslationSession> Sessions => Set<TranslationSession>();
    public DbSet<TranslationRequest> Requests => Set<TranslationRequest>();
    public DbSet<TranslationResult> Results => Set<TranslationResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Carga todas las clases IEntityTypeConfiguration de la carpeta Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
