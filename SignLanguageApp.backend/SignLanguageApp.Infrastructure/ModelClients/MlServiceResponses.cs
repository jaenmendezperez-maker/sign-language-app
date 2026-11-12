using System.Text.Json.Serialization;

namespace SignLanguageApp.Infrastructure.ModelClients;

// Respuestas JSON de ml-service (Python usa snake_case, por eso los JsonPropertyName).
// Deben coincidir con ml-service/app/schemas/prediction_response.py.

public sealed record LsmToTextResponse(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("confidence")] double Confidence);

public sealed record LsmToLsuResponse(
    [property: JsonPropertyName("glosses_lsm")] List<string> GlossesLsm,
    [property: JsonPropertyName("glosses_lsu")] List<string> GlossesLsu,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("confidence")] double Confidence);
