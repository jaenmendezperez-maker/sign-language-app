using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Application.Interfaces;

public interface ITranslationRepository
{
    Task<TranslationSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default);

    Task AddSessionAsync(TranslationSession session, CancellationToken ct = default);

    Task<IReadOnlyList<TranslationHistoryItem>> GetHistoryAsync(int take, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
