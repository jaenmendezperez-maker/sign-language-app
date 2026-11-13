using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.Exceptions;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Domain.Entities;

/// <summary>
/// CONSULTA: sesión de traducción de un médico (RF-12). Es la raíz de agregado de sus
/// traducciones (composición): solo se agregan o modifican a través de ella.
/// </summary>
public class Consultation
{
    private readonly List<TranslationRecord> _records = [];

    public Guid Id { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateOnly Date { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public ConsultationStatus Status { get; private set; }

    public IReadOnlyCollection<TranslationRecord> Records => _records;

    private Consultation() { } // EF Core

    public static Consultation Start(Guid doctorId, DateTime now) => new()
    {
        Id = Guid.NewGuid(),
        DoctorId = doctorId,
        Date = DateOnly.FromDateTime(now),
        StartedAt = now,
        Status = ConsultationStatus.Active,
    };

    public void Pause()
    {
        EnsureStatus(ConsultationStatus.Active, "Solo se puede pausar una consulta activa.");
        Status = ConsultationStatus.Paused;
    }

    public void Resume()
    {
        EnsureStatus(ConsultationStatus.Paused, "Solo se puede reanudar una consulta en pausa.");
        Status = ConsultationStatus.Active;
    }

    /// <summary>RN-03 y RN-06: las palabras pendientes quedan como no confirmadas y se registra la hora de fin.</summary>
    public void Finish(DateTime now)
    {
        if (Status == ConsultationStatus.Finished)
            throw new DomainException("La consulta ya está finalizada.");

        foreach (var record in _records.Where(r => r.Status == RecordStatus.Pending))
            record.MarkNotConfirmed();

        Status = ConsultationStatus.Finished;
        EndedAt = now;
    }

    /// <summary>
    /// RN-01, RN-02, RN-04 y RN-06: registra el resultado del reconocimiento con el nivel y
    /// estado que dictan los umbrales. Si la certeza es baja no se guarda ninguna palabra.
    /// </summary>
    public TranslationRecord AddRecord(Sign? sign, double score, DateTime recognizedAt, CertaintyThresholds thresholds)
    {
        EnsureStatus(ConsultationStatus.Active, "Solo se registran traducciones mientras la consulta está activa.");

        var level = thresholds.LevelFor(score);
        var recognized = level != CertaintyLevel.Low ? sign : null;
        if (level != CertaintyLevel.Low && sign is null)
            throw new DomainException("Falta la seña reconocida.");

        var record = TranslationRecord.Create(Id, recognized?.Id, recognized?.Word, score, level, recognizedAt);
        _records.Add(record);
        return record;
    }

    public TranslationRecord GetRecord(Guid recordId) =>
        _records.FirstOrDefault(r => r.Id == recordId)
        ?? throw new DomainException("La traducción no pertenece a esta consulta.");

    /// <summary>RN-08: limpiar el historial elimina las traducciones de la consulta.</summary>
    public void ClearRecords() => _records.Clear();

    private void EnsureStatus(ConsultationStatus expected, string message)
    {
        if (Status != expected) throw new DomainException(message);
    }
}
