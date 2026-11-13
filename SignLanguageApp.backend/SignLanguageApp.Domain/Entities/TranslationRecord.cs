using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.Exceptions;

namespace SignLanguageApp.Domain.Entities;

/// <summary>TRADUCCION_REGISTRADA: un resultado del reconocimiento dentro de una consulta (RF-09).</summary>
public class TranslationRecord
{
    public Guid Id { get; private set; }
    public Guid ConsultationId { get; private set; }
    public Guid? SignId { get; private set; }
    /// <summary>Copia de la palabra al reconocerla; nula si no se reconoció.</summary>
    public string? Word { get; private set; }
    public DateTime RecognizedAt { get; private set; }
    public decimal SimilarityScore { get; private set; }
    public CertaintyLevel CertaintyLevel { get; private set; }
    public RecordStatus Status { get; private set; }

    private TranslationRecord() { } // EF Core

    internal static TranslationRecord Create(
        Guid consultationId, Guid? signId, string? word, double score, CertaintyLevel level, DateTime recognizedAt)
    {
        if (score is < 0 or > 100 || double.IsNaN(score))
            throw new DomainException("El puntaje de similitud debe estar entre 0 y 100.");

        return new TranslationRecord
        {
            Id = Guid.NewGuid(),
            ConsultationId = consultationId,
            SignId = signId,
            Word = word,
            RecognizedAt = recognizedAt,
            SimilarityScore = Math.Round((decimal)score, 2),
            CertaintyLevel = level,
            Status = ValueObjects.CertaintyThresholds.InitialStatusFor(level),
        };
    }

    /// <summary>Palabras que forman parte de la frase traducida.</summary>
    public bool IsInPhrase => Status is RecordStatus.Accepted or RecordStatus.Confirmed or RecordStatus.NotConfirmed;

    public void Confirm() => Transition(RecordStatus.Confirmed);
    public void Discard() => Transition(RecordStatus.Discarded);
    public void MarkNotConfirmed() => Transition(RecordStatus.NotConfirmed);

    /// <summary>RN-03: solo una palabra pendiente (certeza media) se puede confirmar, descartar o expirar.</summary>
    private void Transition(RecordStatus next)
    {
        if (Status != RecordStatus.Pending)
            throw new DomainException("Solo se puede revisar una palabra con certeza media pendiente.");
        Status = next;
    }
}
