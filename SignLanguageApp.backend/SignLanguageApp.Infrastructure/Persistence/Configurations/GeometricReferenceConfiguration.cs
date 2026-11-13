using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

public sealed class GeometricReferenceConfiguration : IEntityTypeConfiguration<GeometricReference>
{
    public void Configure(EntityTypeBuilder<GeometricReference> b)
    {
        b.ToTable("GeometricReferences", t => t.HasCheckConstraint(
            "CK_GeometricReferences_SampleCount", $"\"SampleCount\" >= {HandGeometry.MinSamples}"));
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedNever();
        b.Property(r => r.Distances).AsJsonb().IsRequired();
        b.Property(r => r.Angles).AsJsonb().IsRequired();
        b.Property(r => r.NormalizedLandmarks).AsJsonb().IsRequired();
        b.HasOne<User>().WithMany().HasForeignKey(r => r.CapturedById).OnDelete(DeleteBehavior.Restrict);
    }
}
