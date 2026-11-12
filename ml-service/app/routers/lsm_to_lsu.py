from typing import Annotated

from fastapi import APIRouter, Depends

from app.dependencies import get_lsm_lsu_model
from app.models.lsm_lsu_model import LsmLsuModel
from app.preprocessing.landmarks import prepare
from app.schemas import LsmToLsuResponse, PredictionRequest

router = APIRouter(prefix="/lsm-to-lsu", tags=["LSM → LSU"])


@router.post("/predict", response_model=LsmToLsuResponse)
def predict(
    request: PredictionRequest,
    model: Annotated[LsmLsuModel, Depends(get_lsm_lsu_model)],
) -> LsmToLsuResponse:
    t = model.predict(prepare(request.frames))
    return LsmToLsuResponse(
        glosses_lsm=t.glosses_lsm,
        glosses_lsu=t.glosses_lsu,
        text=t.text,
        confidence=t.confidence,
    )
