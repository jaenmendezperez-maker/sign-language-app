using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.API.DTOs;

/// <summary>Lo que manda el frontend (por SignalR o por POST).</summary>
public sealed record TranslationRequestDto(
    Guid? SessionId,
    Language Source,
    Language Target,
    float[][][] Frames);
