using Microsoft.Extensions.Options;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Errors;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Application.UseCases.Consultations;

/// <summary>
/// CU-05 Traducir seña / CU-06 Solicitar repetición: guarda en el historial el resultado que
/// calculó el navegador. El servidor vuelve a calcular nivel y estado con los umbrales
/// configurados (no confía en el nivel que mande el cliente) y toma la palabra del catálogo.
/// </summary>
public sealed class RecordTranslationUseCase(
    IConsultationRepository consultations,
    ICatalogRepository catalog,
    IUnitOfWork uow,
    IOptions<CertaintyOptions> options,
    TimeProvider clock)
{
    /// <summary>Tolerancia para la hora que manda el navegador (relojes desfasados).</summary>
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

    public async Task<RecordDto> ExecuteAsync(
        Guid consultationId, Guid doctorId, RecordTranslationCommand command, CancellationToken ct = default)
    {
        if (double.IsNaN(command.SimilarityScore) || command.SimilarityScore is < 0 or > 100)
            throw new RequestValidationException("El puntaje de similitud debe estar entre 0 y 100.");

        var thresholds = options.Value.ToThresholds();
        var consultation = await consultations.GetOwnedAsync(consultationId, doctorId, ct);

        Sign? sign = null;
        if (thresholds.LevelFor(command.SimilarityScore) != CertaintyLevel.Low)
        {
            if (command.SignId is not { } signId)
                throw new RequestValidationException("Falta la seña reconocida.");
            sign = await catalog.GetSignAsync(signId, ct) ?? throw new RequestValidationException("La seña no existe.");
        }

        var record = consultation.AddRecord(sign, command.SimilarityScore, RecognizedAt(command.RecognizedAt), thresholds);
        await uow.SaveChangesAsync(ct);
        return record.ToDto();
    }

    private DateTime RecognizedAt(DateTime? sent)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        if (sent is not { } value) return now;
        var utc = value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
        return (utc - now).Duration() <= ClockSkew ? utc : now;
    }
}
