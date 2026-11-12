using Microsoft.Extensions.DependencyInjection;
using SignLanguageApp.Application.UseCases;

namespace SignLanguageApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<TranslateSignLanguageUseCase>();
        services.AddScoped<GetAvailableLanguagesUseCase>();
        services.AddScoped<GetTranslationHistoryUseCase>();
        return services;
    }
}
