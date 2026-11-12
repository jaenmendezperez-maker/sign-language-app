namespace SignLanguageApp.API.DTOs;

/// <summary>Lo que recibe el frontend.</summary>
public sealed record TranslationResponseDto(
    Guid SessionId,
    string Text,
    IReadOnlyList<string> Glosses,
    double Confidence,
    DateTime Timestamp);
