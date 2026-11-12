using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Application.Interfaces;

public interface ITranslationModelFactory
{
    /// <summary>Pares para los que hay un modelo registrado.</summary>
    IReadOnlyCollection<LanguagePair> AvailablePairs { get; }

    /// <summary>Lanza UnsupportedLanguagePairException si no hay modelo para el par.</summary>
    ITranslationModel Resolve(LanguagePair pair);
}
