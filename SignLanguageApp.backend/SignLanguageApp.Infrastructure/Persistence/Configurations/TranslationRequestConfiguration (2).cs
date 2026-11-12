using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

public sealed class TranslationRequestConfiguration : IEntityTypeConfiguration<TranslationRequest>
{
    public void Configure(EntityTypeBuilder<TranslationRequest> builder)
    {
        builder.ToTable("TranslationRequests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.InputType).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(r => r.Result)
            .WithOne()
            .HasForeignKey<TranslationResult>(res => res.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
