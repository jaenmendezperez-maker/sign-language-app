using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.Errors;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Application.UseCases.Consultations;

internal static class ConsultationAccess
{
    /// <summary>Obtiene la consulta solo si pertenece al médico; si no, responde como si no existiera.</summary>
    public static async Task<Consultation> GetOwnedAsync(
        this IConsultationRepository repository, Guid consultationId, Guid doctorId, CancellationToken ct)
    {
        var consultation = await repository.GetAsync(consultationId, ct);
        if (consultation is null || consultation.DoctorId != doctorId)
            throw new NotFoundException("La consulta no existe.");
        return consultation;
    }
}
