from typing import Annotated

from fastapi import APIRouter, Depends

from app.dependencies import get_lsm_text_model
from app.models.lsm_text_model import LsmTextModel
from app.preprocessing.landmarks import prepare
from app.schemas import LsmToTextResponse, PredictionRequest

router = APIRouter(prefix="/lsm-to-text", tags=["LSM → Texto"])


@router.post("/predict", response_model=LsmToTextResponse)
def predict(
    request: PredictionRequest,
    model: Annotated[LsmTextModel, Depends(get_lsm_text_model)],
) -> LsmToTextResponse:
    p = model.predict(prepare(request.frames))
    return LsmToTextResponse(label=p.label, text=p.text, confidence=p.confidence, top_k=p.top_k)
