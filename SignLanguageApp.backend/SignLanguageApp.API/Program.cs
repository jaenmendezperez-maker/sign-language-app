using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SignLanguageApp.API.Hubs;
using SignLanguageApp.API.Middlewares;
using SignLanguageApp.Application;
using SignLanguageApp.Infrastructure;
using SignLanguageApp.Infrastructure.Persistence;

const string FrontendCors = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// ---------- Capas ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- API ----------
// Enums como texto en JSON: "LSM" en vez de 0
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Una ventana de 30 frames × 75 puntos pesa ~50 KB; el límite por defecto de SignalR es 32 KB
builder.Services.AddSignalR(o => o.MaximumReceiveMessageSize = 512 * 1024)
    .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:4200"];

builder.Services.AddCors(options => options.AddPolicy(FrontendCors, policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials())); // SignalR lo necesita

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json

    // Aplica las migraciones pendientes al arrancar (solo en desarrollo)
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseCors(FrontendCors);

app.MapControllers();
app.MapHub<TranslationHub>("/hubs/translation");

app.Run();
