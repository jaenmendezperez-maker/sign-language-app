using SignLanguageApp.Application.DTOs;

namespace SignLanguageApp.Application.Validators;

/// <summary>
/// Valida la forma de los frames antes de mandarlos al modelo.
/// Debe coincidir con ml-service/app/preprocessing/landmarks.py.
/// </summary>
public static class TranslateCommandValidator
{
    public const int PointsPerFrame = 75; // 33 pose + 21 + 21 manos
    public const int CoordsPerPoint = 3;  // x, y, z
    public const int MaxFrames = 120;

    public static void Validate(TranslateCommand command)
    {
        var frames = command.Frames;

        if (frames is null || frames.Length == 0)
            throw new RequestValidationException("Se necesita al menos un frame.");

        if (frames.Length > MaxFrames)
            throw new RequestValidationException($"Máximo {MaxFrames} frames por petición.");

        for (var i = 0; i < frames.Length; i++)
        {
            if (frames[i] is null || frames[i].Length != PointsPerFrame)
                throw new RequestValidationException(
                    $"Frame {i}: se esperaban {PointsPerFrame} puntos, llegaron {frames[i]?.Length ?? 0}.");

            if (frames[i].Any(p => p is null || p.Length != CoordsPerPoint))
                throw new RequestValidationException(
                    $"Frame {i}: cada punto debe tener {CoordsPerPoint} coordenadas.");
        }
    }
}
