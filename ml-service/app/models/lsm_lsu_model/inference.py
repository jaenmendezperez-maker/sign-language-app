from typing import List, Tuple
from app.schemas.translation_schema import Landmark


def predict(landmarks: List[List[Landmark]]) -> Tuple[str, float]:
    # TODO: reemplazar con la carga e inferencia del modelo real
    return "lsu de ejemplo", 0.0