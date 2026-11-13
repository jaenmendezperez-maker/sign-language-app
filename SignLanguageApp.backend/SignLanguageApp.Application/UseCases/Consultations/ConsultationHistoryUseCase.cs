using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;

namespace SignLanguageApp.Application.UseCases.Consultations;

/// <summary>CU-10 Consultar / limpiar historial (RF-09).</summary>
public sealed class ConsultationHistoryUseCase(IConsultationRepository consultations, IUnitOfWork uow)
{
    public async Task<ConsultationDto> GetAsync(Guid id, Guid doctorId, CancellationToken ct = default) =>
        (await consultations.GetOwnedAsync(id, doctorId, ct)).ToDto();

    public async Task<IReadOnlyList<ConsultationSummaryDto>> ListAsync(Guid doctorId, int take = 50, CancellationToken ct = default) =>
        (await consultations.ListByDoctorAsync(doctorId, Math.Clamp(take, 1, 200), ct)).Select(c => c.ToSummary()).ToList();

    public async Task<ConsultationDto> ClearAsync(Guid id, Guid doctorId, CancellationToken ct = default)
    {
        var consultation = await consultations.GetOwnedAsync(id, doctorId, ct);
        consultation.ClearRecords();
        await uow.SaveChangesAsync(ct);
        return consultation.ToDto();
    }
}
