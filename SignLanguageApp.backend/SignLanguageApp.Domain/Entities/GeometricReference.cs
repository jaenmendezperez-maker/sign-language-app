namespace SignLanguageApp.Domain.Entities;

/// <summary>
/// REFERENCIA_GEOMETRICA: promedio de las muestras capturadas por el administrador.
/// Los vectores se guardan como JSON (RF-11, RNF-09). Para dos manos van concatenados:
/// primero la izquierda y luego la derecha.
/// </summary>
public class GeometricReference
{
    public Guid Id { get; private set; }
    public Guid SignId { get; private set; }
    public double[] Distances { get; private set; } = [];
    public double[] Angles { get; private set; } = [];
    /// <summary>21 puntos (x, y, z) por mano, ya normalizados; sirven para dibujar la mano en el catálogo.</summary>
    public double[] NormalizedLandmarks { get; private set; } = [];
    public short SampleCount { get; private set; }
    public DateTime CapturedAt { get; private set; }
    public Guid CapturedById { get; private set; }

    private GeometricReference() { } // EF Core

    internal static GeometricReference Create(
        Guid signId, double[] distances, double[] angles, double[] landmarks, short sampleCount, Guid adminId, DateTime now) => new()
    {
        Id = Guid.NewGuid(),
        SignId = signId,
        Distances = distances,
        Angles = angles,
        NormalizedLandmarks = landmarks,
        SampleCount = sampleCount,
        CapturedAt = now,
        CapturedById = adminId,
    };
}
