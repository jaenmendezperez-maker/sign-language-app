using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

public sealed class TranslationSessionConfiguration : IEntityTypeConfiguration<TranslationSession>
{
    public void Configure(EntityTypeBuilder<TranslationSession> builder)
    {
        builder.ToTable("TranslationSessions");
        builder.HasKey(s => s.Id);

        // Los Id se generan en el dominio (Guid.NewGuid), no en la base de datos.
        // Sin esto EF trataría las entidades nuevas como existentes al guardarlas.
        builder.Property(s => s.Id).ValueGeneratedNever();

        // Guardar enums como texto ("LSM") es más legible que como número
        builder.Property(s => s.Source).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Target).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(s => s.Pair);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Requests)
            .WithOne()
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
