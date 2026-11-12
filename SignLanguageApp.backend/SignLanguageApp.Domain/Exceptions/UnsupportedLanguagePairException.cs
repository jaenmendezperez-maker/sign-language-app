using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Domain.Exceptions;

public sealed class UnsupportedLanguagePairException(LanguagePair pair, string? reason = null)
    : Exception(reason ?? $"El par de idiomas {pair} no está soportado.")
{
    public LanguagePair Pair { get; } = pair;
}
