using SignLanguageApp.Application.Interfaces;
using SignLanguageApp.Domain.Exceptions;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Infrastructure.ModelRegistry;

/// <summary>
/// Recibe todos los ITranslationModel registrados y elige el del par pedido.
/// Agregar un par nuevo = crear un cliente y registrarlo en DependencyInjection.
/// </summary>
public sealed class TranslationModelFactory : ITranslationModelFactory
{
    private readonly Dictionary<LanguagePair, ITranslationModel> _models;

    public TranslationModelFactory(IEnumerable<ITranslationModel> models)
    {
        _models = models.ToDictionary(m => m.SupportedPair);
    }

    public IReadOnlyCollection<LanguagePair> AvailablePairs => _models.Keys;

    public ITranslationModel Resolve(LanguagePair pair) =>
        _models.TryGetValue(pair, out var model)
            ? model
            : throw new UnsupportedLanguagePairException(pair, $"No hay un modelo disponible para {pair}.");
}
