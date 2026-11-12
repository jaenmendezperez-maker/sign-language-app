using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Domain.Entities;

/// <summary>Una sesión de cámara: desde que el usuario empieza a traducir hasta que termina.</summary>
public class TranslationSession
{
    private readonly List<TranslationRequest> _requests = [];

    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; } // null mientras no haya login (Fase 4)
    public Language Source { get; private set; }
    public Language Target { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    public IReadOnlyCollection<TranslationRequest> Requests => _requests;
    public LanguagePair Pair => new(Source, Target);

    private TranslationSession() { } // EF Core

    public static TranslationSession Start(LanguagePair pair, Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Source = pair.Source,
        Target = pair.Target,
        StartedAt = DateTime.UtcNow,
    };

    public TranslationRequest AddRequest(InputType inputType, int frameCount)
    {
        var request = TranslationRequest.Create(Id, inputType, frameCount);
        _requests.Add(request);
        return request;
    }

    public void End() => EndedAt ??= DateTime.UtcNow;
}
