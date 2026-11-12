using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Application.DTOs;

public sealed record TranslationHistoryItem(
    Guid SessionId,
    Language Source,
    Language Target,
    string Text,
    IReadOnlyList<string> Glosses,
    double Confidence,
    DateTime CreatedAt);
