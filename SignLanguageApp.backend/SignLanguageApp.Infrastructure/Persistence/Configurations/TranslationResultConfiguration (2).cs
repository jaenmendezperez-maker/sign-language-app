using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

public sealed class TranslationResultConfiguration : IEntityTypeConfiguration<TranslationResult>
{
    public void Configure(EntityTypeBuilder<TranslationResult> builder)
    {
        builder.ToTable("TranslationResults");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Text).HasMaxLength(500);
        builder.Property(r => r.Glosses); // PostgreSQL lo guarda como text[]
        builder.HasIndex(r => r.CreatedAt);
    }
}
