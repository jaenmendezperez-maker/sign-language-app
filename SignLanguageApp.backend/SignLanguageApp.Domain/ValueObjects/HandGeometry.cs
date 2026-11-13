namespace SignLanguageApp.Domain.ValueObjects;

/// <summary>
/// Tamaño de los vectores geométricos por mano. Debe coincidir con
/// frontend/src/app/recognition/hand-features.ts.
/// </summary>
public static class HandGeometry
{
    public const int LandmarksPerHand = 21;   // MediaPipe Hands
    public const int DistancesPerHand = 12;
    public const int AnglesPerHand = 15;
    public const int CoordinatesPerHand = LandmarksPerHand * 3;
    public const int MinSamples = 5;          // RF-11
}
