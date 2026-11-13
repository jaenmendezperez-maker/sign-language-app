using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

public sealed class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> b)
    {
        b.ToTable("Consultations", t => t.HasCheckConstraint(
            "CK_Consultations_Status", "\"Status\" IN ('Active','Paused','Finished')"));
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever();
        b.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(c => new { c.DoctorId, c.StartedAt });

        b.HasOne<User>().WithMany().HasForeignKey(c => c.DoctorId).OnDelete(DeleteBehavior.Restrict);

        // Composición: las traducciones solo existen dentro de su consulta (RN-08)
        b.HasMany(c => c.Records).WithOne().HasForeignKey(r => r.ConsultationId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(c => c.Records).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
