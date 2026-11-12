from fastapi import FastAPI

from app.dependencies import get_lsm_text_model
from app.routers import lsm_to_lsu, lsm_to_text

app = FastAPI(
    title="Proyecto Señas · ml-service",
    description="Inferencia de modelos de lengua de señas a partir de landmarks.",
    version="0.1.0",
)

app.include_router(lsm_to_text.router)
app.include_router(lsm_to_lsu.router)


@app.get("/health", tags=["Salud"])
def health() -> dict:
    return {"status": "ok", "fake_model": get_lsm_text_model().is_fake}
