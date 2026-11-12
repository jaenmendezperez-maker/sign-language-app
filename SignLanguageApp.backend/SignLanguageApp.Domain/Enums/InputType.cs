namespace SignLanguageApp.Domain.Enums;

/// <summary>Cómo llegó la entrada al sistema.</summary>
public enum InputType
{
    Landmarks, // puntos clave de MediaPipe (lo que usamos)
    Image,
    Video,
}
