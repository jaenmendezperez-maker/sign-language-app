using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.API.DTOs;

public sealed record HistoryItemDto(
    Guid SessionId,
    Language Source,
    Language Target,
    string Text,
    IReadOnlyList<string> Glosses,
    double Confidence,
    DateTime CreatedAt);
