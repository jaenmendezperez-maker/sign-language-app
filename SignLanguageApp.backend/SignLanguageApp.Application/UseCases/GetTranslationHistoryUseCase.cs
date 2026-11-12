using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Interfaces;

namespace SignLanguageApp.Application.UseCases;

public sealed class GetTranslationHistoryUseCase(ITranslationRepository repository)
{
    public Task<IReadOnlyList<TranslationHistoryItem>> ExecuteAsync(int take = 50, CancellationToken ct = default) =>
        repository.GetHistoryAsync(Math.Clamp(take, 1, 200), ct);
}
