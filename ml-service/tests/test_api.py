import numpy as np
from fastapi.testclient import TestClient

from app.main import app
from app.preprocessing.landmarks import POINTS_PER_FRAME, WINDOW_SIZE, prepare

client = TestClient(app)


def fake_frames(n: int = 30) -> list:
    rng = np.random.default_rng(0)
    return rng.random((n, POINTS_PER_FRAME, 3)).round(4).tolist()


def test_health():
    r = client.get("/health")
    assert r.status_code == 200
    assert r.json()["status"] == "ok"


def test_lsm_to_text_returns_prediction():
    r = client.post("/lsm-to-text/predict", json={"frames": fake_frames()})
    assert r.status_code == 200
    body = r.json()
    assert body["label"] == "HOLA"
    assert body["text"] == "hola"
    assert 0 <= body["confidence"] <= 1


def test_lsm_to_lsu_returns_glosses():
    r = client.post("/lsm-to-lsu/predict", json={"frames": fake_frames()})
    assert r.status_code == 200
    assert r.json()["glosses_lsu"] == ["HOLA"]


def test_rejects_wrong_number_of_points():
    bad = [[[0.0, 0.0, 0.0]] * 10]
    r = client.post("/lsm-to-text/predict", json={"frames": bad})
    assert r.status_code == 422


def test_prepare_pads_short_sequences():
    seq = prepare(fake_frames(5))
    assert seq.shape == (WINDOW_SIZE, POINTS_PER_FRAME, 3)
