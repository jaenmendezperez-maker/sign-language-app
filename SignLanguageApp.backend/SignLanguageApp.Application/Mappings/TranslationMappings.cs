using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Application.Mappings;

/// <summary>
/// Mapeos manuales Entity → DTO. Son pocos y explícitos, así que no hace falta AutoMapper.
/// </summary>
public static class TranslationMappings
{
    public static TranslationOutput ToOutput(this TranslationResult result, Guid sessionId) =>
        new(sessionId, result.Text, result.Glosses, result.Confidence, result.CreatedAt);
}
