"""Instancias únicas de los modelos (se cargan una sola vez al arrancar)."""

from functools import lru_cache

from app.models.lsm_lsu_model import LsmLsuModel
from app.models.lsm_text_model import LsmTextModel


@lru_cache
def get_lsm_text_model() -> LsmTextModel:
    return LsmTextModel()


@lru_cache
def get_lsm_lsu_model() -> LsmLsuModel:
    return LsmLsuModel(get_lsm_text_model())
