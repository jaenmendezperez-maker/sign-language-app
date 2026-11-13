using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Application.UseCases.Consultations;

/// <summary>CU-02 Iniciar consulta de traducción.</summary>
public sealed class StartConsultationUseCase(IConsultationRepository consultations, IUnitOfWork uow, TimeProvider clock)
{
    public async Task<ConsultationDto> ExecuteAsync(Guid doctorId, CancellationToken ct = default)
    {
        var consultation = Consultation.Start(doctorId, clock.GetUtcNow().UtcDateTime);
        await consultations.AddAsync(consultation, ct);
        await uow.SaveChangesAsync(ct);
        return consultation.ToDto();
    }
}
