namespace SignLanguageApp.Domain.Enums;

/// <summary>Idiomas que maneja la app: lenguas de señas y texto.</summary>
public enum Language
{
    LSM,         // Lengua de Señas Mexicana
    LSU,         // Lengua de Señas Uruguaya
    SpanishText, // Texto en español
}

public static class LanguageExtensions
{
    public static bool IsSignLanguage(this Language language) =>
        language is Language.LSM or Language.LSU;
}
