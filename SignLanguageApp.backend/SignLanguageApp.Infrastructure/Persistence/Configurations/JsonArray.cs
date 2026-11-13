using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SignLanguageApp.Infrastructure.Persistence.Configurations;

internal static class JsonArray
{
    private static readonly ValueComparer<double[]> Comparer = new(
        (a, b) => a!.SequenceEqual(b!),
        v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())),
        v => v.ToArray());

    /// <summary>Guarda un double[] como columna jsonb (los vectores de la referencia geométrica).</summary>
    public static PropertyBuilder<double[]> AsJsonb(this PropertyBuilder<double[]> property)
    {
        property
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<double[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<double>(),
                Comparer)
            .HasColumnType("jsonb");
        return property;
    }
}
