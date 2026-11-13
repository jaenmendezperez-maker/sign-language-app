using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;

namespace SignLanguageApp.Application.UseCases.Consultations;

/// <summary>CU-03 Pausar / reanudar consulta.</summary>
public sealed class PauseResumeConsultationUseCase(IConsultationRepository consultations, IUnitOfWork uow)
{
    public Task<ConsultationDto> PauseAsync(Guid id, Guid doctorId, CancellationToken ct = default) =>
        ChangeAsync(id, doctorId, c => c.Pause(), ct);

    public Task<ConsultationDto> ResumeAsync(Guid id, Guid doctorId, CancellationToken ct = default) =>
        ChangeAsync(id, doctorId, c => c.Resume(), ct);

    private async Task<ConsultationDto> ChangeAsync(
        Guid id, Guid doctorId, Action<Domain.Entities.Consultation> change, CancellationToken ct)
    {
        var consultation = await consultations.GetOwnedAsync(id, doctorId, ct);
        change(consultation);
        await uow.SaveChangesAsync(ct);
        return consultation.ToDto();
    }
}
