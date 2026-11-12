using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.Exceptions;

namespace SignLanguageApp.Domain.ValueObjects;

/// <summary>
/// Par origen → destino. Es inmutable y se compara por valor
/// (dos pares con los mismos idiomas son iguales).
/// </summary>
public sealed record LanguagePair(Language Source, Language Target)
{
    /// <summary>
    /// Crea un par validando las reglas de dominio. Que exista un modelo para el par
    /// es otra cosa: eso lo decide ITranslationModelFactory.
    /// </summary>
    public static LanguagePair Create(Language source, Language target)
    {
        var pair = new LanguagePair(source, target);

        if (source == target)
            throw new UnsupportedLanguagePairException(pair, "El origen y el destino no pueden ser iguales.");

        if (!source.IsSignLanguage())
            throw new UnsupportedLanguagePairException(pair, "El origen debe ser una lengua de señas.");

        return pair;
    }

    public override string ToString() => $"{Source}->{Target}";
}
