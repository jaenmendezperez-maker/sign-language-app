using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Infrastructure.Persistence;

/// <summary>
/// Datos iniciales para desarrollo: categorías del vocabulario, un usuario por rol y, si
/// Seed:DemoSigns está activo y el catálogo está vacío, unas señas de demostración (posturas
/// simples de la mano, no señas oficiales de LSM) para poder probar sin registrar nada.
/// Las señas reales las registra el administrador con la cámara (CU-13).
/// </summary>
public sealed class DevelopmentSeeder(AppDbContext db, IPasswordHasher hasher, IConfiguration config, TimeProvider clock)
{
    public static readonly (string Name, string Description)[] Categories =
    [
        ("Síntomas", "Dolor, fiebre, mareo…"),
        ("Urgencias", "Ayuda, emergencia, ambulancia…"),
        ("Respuestas", "Sí, no, gracias…"),
        ("Partes del cuerpo", "Cabeza, pecho, estómago…"),
        ("Lugares", "Hospital, farmacia, baño…"),
    ];

    public const string DemoCategory = "Demostración";
    private const string DemoCategoryDescription =
        "Posturas simples de la mano para probar el sistema. No son señas oficiales de LSM: reemplácelas registrando las señas reales.";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        foreach (var (name, description) in Categories)
            if (!await db.Categories.AnyAsync(c => c.Name == name, ct))
                db.Categories.Add(Category.Create(name, description));

        await SeedUserAsync("Seed:Admin", UserRole.Administrator, ct);
        await SeedUserAsync("Seed:Doctor", UserRole.MedicalStaff, ct);
        await db.SaveChangesAsync(ct);

        if (config.GetValue<bool>("Seed:DemoSigns") && !await db.Signs.AnyAsync(ct))
            await SeedDemoSignsAsync(ct);
    }

    private sealed record DemoSign(
        string Word, short HandCount, double[] Distances, double[] Angles, double[] NormalizedLandmarks, short SampleCount);

    private async Task SeedDemoSignsAsync(CancellationToken ct)
    {
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Administrator, ct);
        if (admin is null) return;

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == DemoCategory, ct);
        if (category is null)
        {
            category = Category.Create(DemoCategory, DemoCategoryDescription);
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        await using var stream = typeof(DevelopmentSeeder).Assembly.GetManifestResourceStream("SignLink.DemoSigns.json")
                                 ?? throw new InvalidOperationException("No se encontró demo-signs.json embebido.");
        var demos = await JsonSerializer.DeserializeAsync<DemoSign[]>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web), ct) ?? [];

        var now = clock.GetUtcNow().UtcDateTime;
        foreach (var d in demos)
        {
            var sign = Sign.Create(d.Word, category.Id, d.HandCount, null, admin.Id, now);
            sign.AddReference(d.Distances, d.Angles, d.NormalizedLandmarks, d.SampleCount, admin.Id, now);
            db.Signs.Add(sign);
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedUserAsync(string section, UserRole role, CancellationToken ct)
    {
        var email = config[$"{section}:Email"];
        var password = config[$"{section}:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var normalized = User.NormalizeEmail(email);
        if (await db.Users.AnyAsync(u => u.Email == normalized, ct)) return;

        var user = User.Create(config[$"{section}:Name"] ?? email, email, "-", role, clock.GetUtcNow().UtcDateTime);
        user.ChangePasswordHash(hasher.Hash(user, password));
        db.Users.Add(user);
    }
}
