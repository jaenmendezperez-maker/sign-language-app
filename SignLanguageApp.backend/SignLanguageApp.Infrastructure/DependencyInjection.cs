using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignLanguageApp.Application.Interfaces;
using SignLanguageApp.Infrastructure.ModelClients;
using SignLanguageApp.Infrastructure.ModelRegistry;
using SignLanguageApp.Infrastructure.Persistence;
using SignLanguageApp.Infrastructure.Persistence.Repositories;

namespace SignLanguageApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ---------- Base de datos ----------
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:Default en appsettings.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ITranslationRepository, TranslationRepository>();

        // ---------- Clientes de ml-service ----------
        var ml = configuration.GetSection(MlServiceOptions.SectionName).Get<MlServiceOptions>() ?? new MlServiceOptions();

        void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri(ml.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(ml.TimeoutSeconds);
        }

        services.AddHttpClient<LsmToTextModelClient>(ConfigureClient);
        services.AddHttpClient<LsmToLsuModelClient>(ConfigureClient);

        // Cada cliente se registra también como ITranslationModel para que la factory los reciba todos
        services.AddTransient<ITranslationModel>(sp => sp.GetRequiredService<LsmToTextModelClient>());
        services.AddTransient<ITranslationModel>(sp => sp.GetRequiredService<LsmToLsuModelClient>());

        services.AddScoped<ITranslationModelFactory, TranslationModelFactory>();

        return services;
    }
}
