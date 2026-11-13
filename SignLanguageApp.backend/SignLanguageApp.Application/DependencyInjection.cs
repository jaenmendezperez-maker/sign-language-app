using Microsoft.Extensions.DependencyInjection;
using SignLanguageApp.Application.UseCases.Auth;
using SignLanguageApp.Application.UseCases.Catalog;
using SignLanguageApp.Application.UseCases.Consultations;

namespace SignLanguageApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<LoginUseCase>();
        services.AddScoped<GetCatalogUseCase>();
        services.AddScoped<RegisterSignReferenceUseCase>();
        services.AddScoped<StartConsultationUseCase>();
        services.AddScoped<PauseResumeConsultationUseCase>();
        services.AddScoped<FinishConsultationUseCase>();
        services.AddScoped<RecordTranslationUseCase>();
        services.AddScoped<ReviewTranslationUseCase>();
        services.AddScoped<ConsultationHistoryUseCase>();
        services.AddScoped<ExportSummaryUseCase>();
        return services;
    }
}
