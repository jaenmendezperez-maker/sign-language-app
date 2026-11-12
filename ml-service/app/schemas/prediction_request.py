from pydantic import BaseModel, Field, field_validator

from app.preprocessing.landmarks import COORDS_PER_POINT, POINTS_PER_FRAME


class PredictionRequest(BaseModel):
    """Ventana de frames con landmarks de MediaPipe.

    Forma esperada: frames[n_frames][75][3]  ->  (x, y, z) por punto.
    """

    frames: list[list[list[float]]] = Field(..., min_length=1, max_length=120)

    @field_validator("frames")
    @classmethod
    def check_shape(cls, frames: list[list[list[float]]]) -> list[list[list[float]]]:
        for i, frame in enumerate(frames):
            if len(frame) != POINTS_PER_FRAME:
                raise ValueError(
                    f"frame {i}: se esperaban {POINTS_PER_FRAME} puntos, llegaron {len(frame)}"
                )
            for j, point in enumerate(frame):
                if len(point) != COORDS_PER_POINT:
                    raise ValueError(
                        f"frame {i}, punto {j}: se esperaban {COORDS_PER_POINT} coordenadas"
                    )
        return frames
