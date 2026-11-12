using System.Net.Http.Json;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Interfaces;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Infrastructure.ModelClients;

/// <summary>
/// Lógica común para llamar a un endpoint de ml-service.
/// Cada cliente concreto solo dice su par, su ruta y cómo traducir la respuesta.
/// </summary>
public abstract class MlServiceClientBase<TResponse>(HttpClient http, string path) : ITranslationModel
{
    public abstract LanguagePair SupportedPair { get; }

    public async Task<TranslationModelResult> TranslateAsync(TranslationModelRequest request, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync(path, new { frames = request.Frames }, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TResponse>(ct)
                   ?? throw new InvalidOperationException($"ml-service devolvió una respuesta vacía en '{path}'.");

        return Map(body);
    }

    protected abstract TranslationModelResult Map(TResponse body);
}
