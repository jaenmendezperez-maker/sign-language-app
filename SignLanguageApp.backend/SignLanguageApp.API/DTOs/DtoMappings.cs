using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.API.DTOs;

/// <summary>Conversión entre los DTOs públicos (API) y los internos (Application).</summary>
public static class DtoMappings
{
    public static TranslateCommand ToCommand(this TranslationRequestDto dto) =>
        new(dto.SessionId, dto.Source, dto.Target, dto.Frames);

    public static TranslationResponseDto ToDto(this TranslationOutput output) =>
        new(output.SessionId, output.Text, output.Glosses, output.Confidence, output.Timestamp);

    public static LanguagePairDto ToDto(this LanguagePair pair) =>
        new(pair.Source, pair.Target);

    public static HistoryItemDto ToDto(this TranslationHistoryItem item) =>
        new(item.SessionId, item.Source, item.Target, item.Text, item.Glosses, item.Confidence, item.CreatedAt);
}
