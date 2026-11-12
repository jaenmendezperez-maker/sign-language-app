from pydantic import BaseModel


class LsmToTextResponse(BaseModel):
    label: str                              # glosa reconocida, ej. "HOLA"
    text: str                               # texto en español, ej. "hola"
    confidence: float                       # 0.0 – 1.0
    top_k: list[tuple[str, float]]          # alternativas ordenadas


class LsmToLsuResponse(BaseModel):
    glosses_lsm: list[str]                  # lo que se reconoció en LSM
    glosses_lsu: list[str]                  # su equivalente en LSU
    text: str                               # traducción al español (referencia)
    confidence: float
