using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Infrastructure.ModelClients;

/// <summary>LSM → LSU. Llama a POST /lsm-to-lsu/predict y devuelve las glosas LSU.</summary>
public sealed class LsmToLsuModelClient(HttpClient http)
    : MlServiceClientBase<LsmToLsuResponse>(http, "lsm-to-lsu/predict")
{
    public override LanguagePair SupportedPair { get; } = new(Language.LSM, Language.LSU);

    protected override TranslationModelResult Map(LsmToLsuResponse body) =>
        new(body.Text, body.GlossesLsu, body.Confidence);
}
