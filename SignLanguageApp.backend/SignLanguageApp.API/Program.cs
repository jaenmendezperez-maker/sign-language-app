using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SignLanguageApp.API.Middlewares;
using SignLanguageApp.Application;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Infrastructure;
using SignLanguageApp.Infrastructure.Persistence;
using SignLanguageApp.Infrastructure.Security;

const string FrontendCors = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// ---------- Capas ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOptions<CertaintyOptions>()
    .Bind(builder.Configuration.GetSection(CertaintyOptions.SectionName))
    .Validate(o => o.Medium > 0 && o.High > o.Medium && o.High <= 100, "Certainty: debe cumplirse 0 < Medium < High ≤ 100.")
    .Validate(o => o.ConfirmationSeconds > 0, "Certainty:ConfirmationSeconds debe ser mayor a 0.")
    .ValidateOnStart();

// ---------- Autenticación (JWT) y autorización por rol ----------
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // conserva "sub" y el claim de rol tal como se emitieron
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = JwtTokenService.SigningKey(jwt),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = "name",
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

// ---------- API ----------
// Enums como texto en JSON: "MedicalStaff" en vez de 1
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(options => options.AddPolicy(FrontendCors, policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Content-Disposition"))); // para el nombre del .txt exportado

var app = builder.Build();

// El logging va primero para que también registre las peticiones que terminan en error
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json

    // Aplica las migraciones y siembra categorías y usuarios de prueba (solo en desarrollo)
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DevelopmentSeeder>().SeedAsync();
}

app.UseCors(FrontendCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Visible para las pruebas de integración (WebApplicationFactory).</summary>
public partial class Program;
