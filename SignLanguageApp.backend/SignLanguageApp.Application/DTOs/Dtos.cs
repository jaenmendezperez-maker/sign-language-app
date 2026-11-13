using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Application.DTOs;

// ---------- Auth ----------
public sealed record LoginCommand(string Email, string Password);
public sealed record LoginResult(string Token, DateTime ExpiresAt, string Name, UserRole Role);

// ---------- Catálogo ----------
public sealed record CategoryDto(int Id, string Name, string? Description);

public sealed record ReferenceDto(Guid Id, double[] Distances, double[] Angles, double[] NormalizedLandmarks, short SampleCount, DateTime CapturedAt);

public sealed record SignDto(
    Guid Id, string Word, int CategoryId, string CategoryName, short HandCount, string? VisualUrl, IReadOnlyList<ReferenceDto> References);

public sealed record RegisterReferenceCommand(
    string Word,
    int CategoryId,
    short HandCount,
    string? VisualUrl,
    double[] Distances,
    double[] Angles,
    double[] NormalizedLandmarks,
    short SampleCount);

// ---------- Consultas ----------
public sealed record RecordDto(
    Guid Id, Guid? SignId, string? Word, DateTime RecognizedAt, decimal SimilarityScore, CertaintyLevel CertaintyLevel, RecordStatus Status);

public sealed record ConsultationDto(
    Guid Id, DateOnly Date, DateTime StartedAt, DateTime? EndedAt, ConsultationStatus Status, IReadOnlyList<RecordDto> Records);

public sealed record ConsultationSummaryDto(
    Guid Id, DateOnly Date, DateTime StartedAt, DateTime? EndedAt, ConsultationStatus Status, int WordCount);

/// <summary>Lo que manda el navegador cuando completa una seña (solo texto y métricas, nunca video).</summary>
public sealed record RecordTranslationCommand(Guid? SignId, double SimilarityScore, DateTime? RecognizedAt);

public static class DtoMappings
{
    public static CategoryDto ToDto(this Category c) => new(c.Id, c.Name, c.Description);

    public static SignDto ToDto(this Sign s) => new(
        s.Id, s.Word, s.CategoryId, s.Category?.Name ?? string.Empty, s.HandCount, s.VisualUrl,
        s.References.OrderBy(r => r.CapturedAt)
            .Select(r => new ReferenceDto(r.Id, r.Distances, r.Angles, r.NormalizedLandmarks, r.SampleCount, r.CapturedAt))
            .ToList());

    public static RecordDto ToDto(this TranslationRecord r) =>
        new(r.Id, r.SignId, r.Word, r.RecognizedAt, r.SimilarityScore, r.CertaintyLevel, r.Status);

    public static ConsultationDto ToDto(this Consultation c) => new(
        c.Id, c.Date, c.StartedAt, c.EndedAt, c.Status,
        c.Records.OrderBy(r => r.RecognizedAt).Select(r => r.ToDto()).ToList());

    public static ConsultationSummaryDto ToSummary(this Consultation c) =>
        new(c.Id, c.Date, c.StartedAt, c.EndedAt, c.Status, c.Records.Count(r => r.IsInPhrase));
}
