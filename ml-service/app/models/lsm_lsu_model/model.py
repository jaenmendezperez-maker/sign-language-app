"""LSM -> LSU como pipeline: reconocer LSM y traducir las glosas a LSU.

Reutiliza el reconocedor de LSM; lo nuevo es el diccionario de glosas.
Para el MVP basta un diccionario; más adelante puede ser un modelo seq2seq.
"""

from dataclasses import dataclass

import numpy as np

from app.models.lsm_text_model import LsmTextModel

# Glosa LSM -> glosa LSU (por ahora coinciden; aquí van las diferencias reales)
LSM_TO_LSU: dict[str, str] = {
    "HOLA": "HOLA",
    "GRACIAS": "GRACIAS",
    "ADIOS": "CHAU",
}

@dataclass(frozen=True)
class LsuTranslation:
    glosses_lsm: list[str]
    glosses_lsu: list[str]
    text: str
    confidence: float

class LsmLsuModel:
    def __init__(self, recognizer: LsmTextModel) -> None:
        self._recognizer = recognizer

    def predict(self, seq: np.ndarray) -> LsuTranslation:
        p = self._recognizer.predict(seq)
        lsu = LSM_TO_LSU.get(p.label, p.label)
        return LsuTranslation([p.label], [lsu], p.text, p.confidence)