using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Infrastructure.ModelClients;

/// <summary>LSM → texto en español. Llama a POST /lsm-to-text/predict.</summary>
public sealed class LsmToTextModelClient(HttpClient http)
    : MlServiceClientBase<LsmToTextResponse>(http, "lsm-to-text/predict")
{
    public override LanguagePair SupportedPair { get; } = new(Language.LSM, Language.SpanishText);

    protected override TranslationModelResult Map(LsmToTextResponse body) =>
        new(body.Text, [body.Label], body.Confidence);
}
