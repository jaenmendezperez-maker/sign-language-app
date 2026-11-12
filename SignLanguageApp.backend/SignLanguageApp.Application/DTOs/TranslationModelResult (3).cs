namespace SignLanguageApp.Application.DTOs;

/// <summary>Lo que devuelve cualquier modelo, ya en un formato común.</summary>
public sealed record TranslationModelResult(string Text, IReadOnlyList<string> Glosses, double Confidence);
