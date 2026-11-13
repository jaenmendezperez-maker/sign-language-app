using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

public sealed class SignConfiguration : IEntityTypeConfiguration<Sign>
{
    public void Configure(EntityTypeBuilder<Sign> b)
    {
        b.ToTable("Signs", t => t.HasCheckConstraint("CK_Signs_HandCount", "\"HandCount\" IN (1, 2)"));
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).ValueGeneratedNever();
        b.Property(s => s.Word).HasMaxLength(60).IsRequired();
        b.HasIndex(s => s.Word).IsUnique();
        b.Property(s => s.VisualUrl).HasMaxLength(500);

        b.HasOne(s => s.Category).WithMany().HasForeignKey(s => s.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(s => s.CreatedById).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(s => s.References).WithOne().HasForeignKey(r => r.SignId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(s => s.References).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
