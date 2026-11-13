using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).UseIdentityByDefaultColumn();
        b.Property(c => c.Name).HasMaxLength(60).IsRequired();
        b.HasIndex(c => c.Name).IsUnique();
        b.Property(c => c.Description).HasMaxLength(250);
    }
}
