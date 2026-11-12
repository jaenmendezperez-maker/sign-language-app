using SignLanguageApp.Application.Interfaces;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Application.UseCases;

public sealed class GetAvailableLanguagesUseCase(ITranslationModelFactory modelFactory)
{
    public IReadOnlyCollection<LanguagePair> Execute() => modelFactory.AvailablePairs;
}
