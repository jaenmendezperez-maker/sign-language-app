using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Errors;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Application.UseCases.Catalog;

/// <summary>
/// CU-13 Registrar referencia geométrica (RF-11). Si la palabra ya existe, la referencia se
/// agrega como variante de esa seña. El navegador manda el promedio de las muestras, nunca imágenes.
/// </summary>
public sealed class RegisterSignReferenceUseCase(ICatalogRepository catalog, IUnitOfWork uow, TimeProvider clock)
{
    public const int MaxWordLength = 60;

    public async Task<SignDto> ExecuteAsync(RegisterReferenceCommand command, Guid adminId, CancellationToken ct = default)
    {
        Validate(command);

        var category = await catalog.GetCategoryAsync(command.CategoryId, ct)
                       ?? throw new RequestValidationException("La categoría no existe.");

        var now = clock.GetUtcNow().UtcDateTime;
        var sign = await catalog.GetSignByWordAsync(Sign.NormalizeWord(command.Word), ct);

        if (sign is null)
        {
            sign = Sign.Create(command.Word, category.Id, command.HandCount, command.VisualUrl, adminId, now);
            await catalog.AddSignAsync(sign, ct);
        }
        else if (sign.HandCount != command.HandCount)
        {
            throw new RequestValidationException(
                $"La seña «{sign.Word}» ya existe con {sign.HandCount} mano(s); la nueva captura usa {command.HandCount}.");
        }

        sign.AddReference(command.Distances, command.Angles, command.NormalizedLandmarks, command.SampleCount, adminId, now);
        await uow.SaveChangesAsync(ct);

        return (await catalog.GetSignAsync(sign.Id, ct) ?? sign).ToDto();
    }

    private static void Validate(RegisterReferenceCommand c)
    {
        if (string.IsNullOrWhiteSpace(c.Word) || c.Word.Trim().Length > MaxWordLength)
            throw new RequestValidationException($"La palabra es obligatoria (máximo {MaxWordLength} caracteres).");
        if (c.HandCount is not (1 or 2))
            throw new RequestValidationException("El número de manos debe ser 1 o 2.");
        if (c.Distances is null || c.Angles is null || c.NormalizedLandmarks is null)
            throw new RequestValidationException("Faltan los vectores de la referencia.");
        if (c.SampleCount < HandGeometry.MinSamples)
            throw new RequestValidationException($"Se necesitan al menos {HandGeometry.MinSamples} muestras.");
        if (c.Distances.Length != HandGeometry.DistancesPerHand * c.HandCount
            || c.Angles.Length != HandGeometry.AnglesPerHand * c.HandCount
            || c.NormalizedLandmarks.Length != HandGeometry.CoordinatesPerHand * c.HandCount)
            throw new RequestValidationException(
                $"Los vectores no corresponden a {c.HandCount} mano(s): se esperan " +
                $"{HandGeometry.DistancesPerHand * c.HandCount} distancias, {HandGeometry.AnglesPerHand * c.HandCount} ángulos " +
                $"y {HandGeometry.CoordinatesPerHand * c.HandCount} coordenadas.");
        if (!string.IsNullOrWhiteSpace(c.VisualUrl)
            && (c.VisualUrl.Length > 500
                || !Uri.TryCreate(c.VisualUrl, UriKind.Absolute, out var uri)
                || uri.Scheme is not ("http" or "https")))
            throw new RequestValidationException("La URL de la referencia visual no es válida.");
    }
}
