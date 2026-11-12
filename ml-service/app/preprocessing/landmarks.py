"""Preprocesamiento de landmarks.

IMPORTANTE: training/ debe usar estas MISMAS funciones para que el modelo
vea los datos igual en entrenamiento y en producción.
"""

import numpy as np

POSE_POINTS = 33
HAND_POINTS = 21
POINTS_PER_FRAME = POSE_POINTS + 2 * HAND_POINTS  # 75
COORDS_PER_POINT = 3
WINDOW_SIZE = 30  # frames que espera el modelo (~1 s a 30 fps)

# Índices de los hombros en la pose de MediaPipe
LEFT_SHOULDER, RIGHT_SHOULDER = 11, 12


def to_array(frames: list[list[list[float]]]) -> np.ndarray:
    """Convierte la lista JSON a un array (n_frames, 75, 3)."""
    return np.asarray(frames, dtype=np.float32)


def normalize(seq: np.ndarray) -> np.ndarray:
    """Centra en el punto medio de los hombros y escala por su distancia.

    Así la predicción no depende de dónde esté la persona en la imagen
    ni de qué tan cerca esté de la cámara.
    """
    left = seq[:, LEFT_SHOULDER, :]
    right = seq[:, RIGHT_SHOULDER, :]
    center = (left + right) / 2.0
    scale = np.linalg.norm(left - right, axis=1)
    scale[scale < 1e-6] = 1.0  # evita dividir entre cero si no se detectó pose
    return (seq - center[:, None, :]) / scale[:, None, None]


def pad_or_trim(seq: np.ndarray, size: int = WINDOW_SIZE) -> np.ndarray:
    """Ajusta la secuencia a exactamente `size` frames."""
    n = seq.shape[0]
    if n >= size:
        return seq[-size:]
    padding = np.repeat(seq[-1:], size - n, axis=0)  # repite el último frame
    return np.concatenate([seq, padding], axis=0)


def prepare(frames: list[list[list[float]]]) -> np.ndarray:
    """Pipeline completo: JSON -> array normalizado de forma (30, 75, 3)."""
    return pad_or_trim(normalize(to_array(frames)))
