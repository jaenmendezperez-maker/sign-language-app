using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Application.DTOs;

/// <summary>Entrada del caso de uso de traducción.</summary>
public sealed record TranslateCommand(
    Guid? SessionId,
    Language Source,
    Language Target,
    float[][][] Frames);

/// <summary>Salida del caso de uso de traducción.</summary>
public sealed record TranslationOutput(
    Guid SessionId,
    string Text,
    IReadOnlyList<string> Glosses,
    double Confidence,
    DateTime Timestamp);
