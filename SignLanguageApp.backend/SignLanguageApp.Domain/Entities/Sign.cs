using SignLanguageApp.Domain.Exceptions;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Domain.Entities;

/// <summary>SENA: palabra del vocabulario base que el sistema puede reconocer.</summary>
public class Sign
{
    private readonly List<GeometricReference> _references = [];

    public Guid Id { get; private set; }
    public string Word { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public short HandCount { get; private set; }
    public string? VisualUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedById { get; private set; }

    public IReadOnlyCollection<GeometricReference> References => _references;

    private Sign() { } // EF Core

    public static Sign Create(string word, int categoryId, short handCount, string? visualUrl, Guid adminId, DateTime now)
    {
        if (handCount is not (1 or 2))
            throw new DomainException("Una seña usa una o dos manos.");

        return new Sign
        {
            Id = Guid.NewGuid(),
            Word = NormalizeWord(word),
            CategoryId = categoryId,
            HandCount = handCount,
            VisualUrl = string.IsNullOrWhiteSpace(visualUrl) ? null : visualUrl.Trim(),
            IsActive = true,
            CreatedAt = now,
            CreatedById = adminId,
        };
    }

    public static string NormalizeWord(string word) => word.Trim().ToLowerInvariant();

    /// <summary>
    /// Agrega una referencia (o una variante si la seña ya tenía otra). RN-05 y RN-09:
    /// mínimo 5 muestras y vectores del tamaño que corresponde al número de manos.
    /// </summary>
    public GeometricReference AddReference(
        double[] distances, double[] angles, double[] normalizedLandmarks, short sampleCount, Guid adminId, DateTime now)
    {
        if (sampleCount < HandGeometry.MinSamples)
            throw new DomainException($"Se necesitan al menos {HandGeometry.MinSamples} muestras.");

        Expect(distances, HandGeometry.DistancesPerHand, "distancias");
        Expect(angles, HandGeometry.AnglesPerHand, "ángulos");
        Expect(normalizedLandmarks, HandGeometry.CoordinatesPerHand, "coordenadas");

        var reference = GeometricReference.Create(Id, distances, angles, normalizedLandmarks, sampleCount, adminId, now);
        _references.Add(reference);
        return reference;
    }

    private void Expect(double[] values, int perHand, string what)
    {
        var expected = perHand * HandCount;
        if (values.Length != expected)
            throw new DomainException($"Se esperaban {expected} {what} para {HandCount} mano(s), llegaron {values.Length}.");
        if (values.Any(v => !double.IsFinite(v)))
            throw new DomainException($"Hay valores inválidos en las {what}.");
    }
}
