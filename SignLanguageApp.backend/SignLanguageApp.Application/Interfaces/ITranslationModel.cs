using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Application.Interfaces;

/// <summary>Cualquier modelo capaz de traducir un par de idiomas.</summary>
public interface ITranslationModel
{
    LanguagePair SupportedPair { get; }

    Task<TranslationModelResult> TranslateAsync(TranslationModelRequest request, CancellationToken ct = default);
}
