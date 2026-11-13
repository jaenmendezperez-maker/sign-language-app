using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

public sealed class TranslationRecordConfiguration : IEntityTypeConfiguration<TranslationRecord>
{
    public void Configure(EntityTypeBuilder<TranslationRecord> b)
    {
        b.ToTable("TranslationRecords", t =>
        {
            t.HasCheckConstraint("CK_TranslationRecords_Score", "\"SimilarityScore\" BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_TranslationRecords_Level", "\"CertaintyLevel\" IN ('High','Medium','Low')");
            t.HasCheckConstraint("CK_TranslationRecords_Status",
                "\"Status\" IN ('Accepted','Pending','Confirmed','NotConfirmed','Discarded','NotRecognized')");
            // RN-04: una seña no reconocida no tiene seña ni palabra
            t.HasCheckConstraint("CK_TranslationRecords_NotRecognized",
                "\"Status\" <> 'NotRecognized' OR (\"SignId\" IS NULL AND \"Word\" IS NULL)");
        });
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedNever();
        b.Property(r => r.Word).HasMaxLength(60);
        b.Property(r => r.SimilarityScore).HasPrecision(5, 2);
        b.Property(r => r.CertaintyLevel).HasConversion<string>().HasMaxLength(10);
        b.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        b.Ignore(r => r.IsInPhrase);
        b.HasIndex(r => new { r.ConsultationId, r.RecognizedAt });

        b.HasOne<Sign>().WithMany().HasForeignKey(r => r.SignId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
    }
}
