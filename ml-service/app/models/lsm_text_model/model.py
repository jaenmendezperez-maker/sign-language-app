"""Reconocedor LSM -> glosa.

Fase 1: modelo FALSO que siempre responde "HOLA". Sirve para probar que los
tres servicios se comunican. En la Fase 3 se reemplaza `predict` por la
inferencia real cargando los pesos desde ml-service/weights/.
"""

from dataclasses import dataclass

import numpy as np

# Glosa LSM -> texto en español
GLOSS_TO_TEXT: dict[str, str] = {
    "HOLA": "hola",
    "GRACIAS": "gracias",
    "ADIOS": "adiós",
}


@dataclass(frozen=True)
class Prediction:
    label: str
    text: str
    confidence: float
    top_k: list[tuple[str, float]]


class LsmTextModel:
    is_fake = True

    def __init__(self, weights_path: str | None = None) -> None:
        # Fase 3: aquí se cargará el modelo, ej.
        #   self._model = keras.models.load_model(weights_path)
        self.weights_path = weights_path

    def predict(self, seq: np.ndarray) -> Prediction:
        """`seq` llega ya preprocesado con forma (30, 75, 3)."""
        top_k = [("HOLA", 0.93), ("GRACIAS", 0.04), ("ADIOS", 0.03)]
        label, confidence = top_k[0]
        return Prediction(label, GLOSS_TO_TEXT[label], confidence, top_k)
