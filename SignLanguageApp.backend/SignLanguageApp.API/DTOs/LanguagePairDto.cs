using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.API.DTOs;

public sealed record LanguagePairDto(Language Source, Language Target);
