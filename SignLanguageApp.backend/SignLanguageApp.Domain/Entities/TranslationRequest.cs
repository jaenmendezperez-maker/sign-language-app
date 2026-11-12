using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Domain.Entities;

/// <summary>Una ventana de frames enviada a traducir dentro de una sesión.</summary>
public class TranslationRequest
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public InputType InputType { get; private set; }
    public int FrameCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public TranslationResult? Result { get; private set; }

    private TranslationRequest() { } // EF Core

    internal static TranslationRequest Create(Guid sessionId, InputType inputType, int frameCount) => new()
    {
        Id = Guid.NewGuid(),
        SessionId = sessionId,
        InputType = inputType,
        FrameCount = frameCount,
        CreatedAt = DateTime.UtcNow,
    };

    public TranslationResult Complete(string text, IEnumerable<string> glosses, double confidence)
    {
        if (Result is not null)
            throw new InvalidOperationException("Esta petición ya tiene un resultado.");

        Result = TranslationResult.Create(Id, text, glosses, confidence);
        return Result;
    }
}
