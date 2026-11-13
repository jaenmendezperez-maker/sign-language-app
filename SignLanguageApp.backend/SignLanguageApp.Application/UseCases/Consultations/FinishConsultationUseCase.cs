using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;

namespace SignLanguageApp.Application.UseCases.Consultations;

/// <summary>CU-04 Finalizar consulta. Las palabras pendientes quedan "no confirmadas".</summary>
public sealed class FinishConsultationUseCase(IConsultationRepository consultations, IUnitOfWork uow, TimeProvider clock)
{
    public async Task<ConsultationDto> ExecuteAsync(Guid id, Guid doctorId, CancellationToken ct = default)
    {
        var consultation = await consultations.GetOwnedAsync(id, doctorId, ct);
        consultation.Finish(clock.GetUtcNow().UtcDateTime);
        await uow.SaveChangesAsync(ct);
        return consultation.ToDto();
    }
}
