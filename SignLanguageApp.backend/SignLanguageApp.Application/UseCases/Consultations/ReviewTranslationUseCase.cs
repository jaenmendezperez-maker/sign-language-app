using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Application.UseCases.Consultations;

/// <summary>CU-07 Confirmar o descartar palabra con certeza media (RF-08).</summary>
public sealed class ReviewTranslationUseCase(IConsultationRepository consultations, IUnitOfWork uow)
{
    public Task<RecordDto> ConfirmAsync(Guid id, Guid recordId, Guid doctorId, CancellationToken ct = default) =>
        ReviewAsync(id, recordId, doctorId, r => r.Confirm(), ct);

    public Task<RecordDto> DiscardAsync(Guid id, Guid recordId, Guid doctorId, CancellationToken ct = default) =>
        ReviewAsync(id, recordId, doctorId, r => r.Discard(), ct);

    /// <summary>Pasaron los segundos de confirmación sin que el médico decidiera.</summary>
    public Task<RecordDto> MarkNotConfirmedAsync(Guid id, Guid recordId, Guid doctorId, CancellationToken ct = default) =>
        ReviewAsync(id, recordId, doctorId, r => r.MarkNotConfirmed(), ct);

    private async Task<RecordDto> ReviewAsync(
        Guid id, Guid recordId, Guid doctorId, Action<TranslationRecord> review, CancellationToken ct)
    {
        var consultation = await consultations.GetOwnedAsync(id, doctorId, ct);
        var record = consultation.GetRecord(recordId);
        review(record);
        await uow.SaveChangesAsync(ct);
        return record.ToDto();
    }
}
