namespace SignLanguageApp.Domain.Entities;

/// <summary>Lo que devolvió el modelo para una petición.</summary>
public class TranslationResult
{
    public Guid Id { get; private set; }
    public Guid RequestId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public List<string> Glosses { get; private set; } = [];
    public double Confidence { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TranslationResult() { } // EF Core

    internal static TranslationResult Create(Guid requestId, string text, IEnumerable<string> glosses, double confidence)
    {
        if (confidence is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(confidence), "La confianza debe estar entre 0 y 1.");

        return new TranslationResult
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            Text = text,
            Glosses = glosses.ToList(),
            Confidence = confidence,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
