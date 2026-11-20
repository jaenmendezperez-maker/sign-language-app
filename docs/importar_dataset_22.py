"""
Importa el corpus de 22 señas de LSM (Zenodo 23744039) a dataset-22-signs.json.

Por cada clip:
  1. Quita el relleno (_len.txt) y usa _mask.npy para saber qué manos se detectaron.
  2. Decide si la seña es de 1 o 2 manos.
  3. Corrige la proporción de la imagen (x y z se multiplican por ancho/alto del video, --aspect).
  4. Busca la ventana de 8 cuadros seguidos más quieta (como SignDetector.stableFrames)
     y promedia sus características (como averageFeatures).

Por cada seña genera UNA REFERENCIA POR INTÉRPRETE (promedio de sus clips), no un promedio
global: promediar manos de personas distintas da una postura que no se parece a ninguna.
El reconocimiento debe quedarse con la referencia más parecida de cada seña.

Las distancias y los ángulos se calculan EXACTAMENTE como en hand-features.ts / hand-geometry.ts.

Uso:
  python docs\\importar_dataset_22.py "D:\\señas\\DataSet_5_SIGNERS" dataset-22-signs.json
"""

import argparse
import json
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np

# ---------------------------------------------------------------- hand-geometry.ts
LANDMARKS_PER_HAND = 21
WRIST, MIDDLE_MCP = 0, 9
FINGERTIPS = [4, 8, 12, 16, 20]
FINGER_CHAINS = [
    [0, 1, 2, 3, 4],
    [0, 5, 6, 7, 8],
    [0, 9, 10, 11, 12],
    [0, 13, 14, 15, 16],
    [0, 17, 18, 19, 20],
]
DISTANCE_PAIRS = (
    [(WRIST, t) for t in FINGERTIPS]
    + [(4, 8), (8, 12), (12, 16), (16, 20)]
    + [(4, 12), (4, 16), (4, 20)]
)

# ---------------------------------------------------------------- dataset
LEFT, RIGHT = slice(0, 63), slice(63, 126)  # columnas 1–63 y 64–126
STABLE_FRAMES = 8   # DEFAULT_DETECTOR.stableFrames
MIN_FRAMES = 3      # si no hay 8 cuadros seguidos, se acepta una ventana más corta
MIN_CLIPS_PER_SIGNER = 3  # menos clips que esto → no se crea referencia para ese intérprete

# Nombre de carpeta → palabra que verá el usuario
DISPLAY = {"como": "cómo", "cuando": "cuándo", "donde": "dónde", "el": "él",
           "perdon": "perdón", "si": "sí", "tu": "tú"}

# Palabra → categoría (debe existir en DevelopmentSeeder)
CATEGORY = {
    "sí": "Respuestas", "no": "Respuestas", "bien": "Respuestas", "mal": "Respuestas",
    "gracias": "Respuestas", "perdón": "Respuestas", "hola": "Respuestas",
    "ayudar": "Urgencias",
    "casa": "Lugares",
    "ahora": "Tiempo", "hoy": "Tiempo", "cuándo": "Tiempo",
    "cómo": "Preguntas", "dónde": "Preguntas",
    "yo": "Personas", "tú": "Personas", "él": "Personas",
    "comer": "Verbos", "ir": "Verbos", "querer": "Verbos", "saber": "Verbos", "tener": "Verbos",
}


# ---------------------------------------------------------------- hand-features.ts
def angle_at(a, b, c):
    u, v = a - b, c - b
    denom = np.linalg.norm(u) * np.linalg.norm(v)
    if denom < 1e-9:
        return 0.0
    cos = np.clip(np.dot(u, v) / denom, -1.0, 1.0)
    return float(np.degrees(np.arccos(cos)))


def normalize_hand(p):
    origin = p[WRIST]
    scale = np.linalg.norm(p[MIDDLE_MCP] - origin) or 1.0
    return (p - origin) / scale


def single_hand_features(p):
    p = normalize_hand(p)
    distances = [float(np.linalg.norm(p[a] - p[b])) for a, b in DISTANCE_PAIRS]
    angles = [angle_at(p[ch[i - 1]], p[ch[i]], p[ch[i + 1]]) for ch in FINGER_CHAINS for i in (1, 2, 3)]
    return distances, angles, p.reshape(-1).tolist()


def frame_features(frame, hands, aspect):
    """hands: lista de slices en orden izquierda → derecha (igual que handFeatures)."""
    d, a, n = [], [], []
    for sl in hands:
        pts = frame[sl].reshape(LANDMARKS_PER_HAND, 3).astype(np.float64).copy()
        pts[:, 0] *= aspect  # x
        pts[:, 2] *= aspect  # z (MediaPipe usa la misma escala que x)
        fd, fa, fn = single_hand_features(pts)
        d += fd; a += fa; n += fn
    return np.array(d), np.array(a), np.array(n)


# ---------------------------------------------------------------- lectura de clips
def load_clips(root):
    for npy in sorted(root.rglob("*.npy")):
        if npy.stem.endswith(("_norm", "_mask")):
            continue
        base = npy.with_suffix("")
        data = np.load(npy)
        mask = np.load(f"{base}_mask.npy").astype(bool)
        length = int(Path(f"{base}_len.txt").read_text().strip())
        signer = npy.relative_to(root).parts[0]
        word = npy.parent.name
        yield signer, word, data[:length], mask[:length]


# ---------------------------------------------------------------- ventana más quieta
def stillest_window(data, valid, hands, aspect):
    def xy(t):
        pts = np.concatenate([data[t, sl].reshape(LANDMARKS_PER_HAND, 3)[:, :2] for sl in hands])
        pts = pts.astype(np.float64).copy()
        pts[:, 0] *= aspect
        return pts

    runs, start = [], None
    for t, ok in enumerate(list(valid) + [False]):
        if ok and start is None:
            start = t
        elif not ok and start is not None:
            runs.append((start, t)); start = None

    best, best_motion = None, np.inf
    for size in (STABLE_FRAMES, MIN_FRAMES):
        for s, e in runs:
            for w in range(s, e - size + 1):
                motion = np.mean([np.mean(np.linalg.norm(xy(t + 1) - xy(t), axis=1)) for t in range(w, w + size - 1)])
                if motion < best_motion:
                    best, best_motion = list(range(w, w + size)), motion
        if best is not None:
            return best
    return None


def clip_reference(data, mask, aspect):
    left, right, both = mask[:, 0], mask[:, 1], mask[:, 0] & mask[:, 1]
    detected = left | right
    if detected.sum() < MIN_FRAMES:
        return None
    if both.sum() >= 0.5 * detected.sum():
        hands, valid, count = [LEFT, RIGHT], both, 2
    elif left.sum() >= right.sum():
        hands, valid, count = [LEFT], left, 1
    else:
        hands, valid, count = [RIGHT], right, 1

    window = stillest_window(data, valid, hands, aspect)
    if window is None:
        return None
    feats = [frame_features(data[t], hands, aspect) for t in window]
    return count, *(np.mean([f[i] for f in feats], axis=0) for i in range(3))


def rounded_mean(rows):
    return [round(float(v), 4) for v in np.mean(rows, axis=0)]


# ---------------------------------------------------------------- main
def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("dataset", type=Path)
    ap.add_argument("salida", type=Path)
    ap.add_argument("--aspect", type=float, default=16 / 9,
                    help="ancho/alto del video original: 1.78 para 16:9 (por defecto), 1.33 para 4:3")
    args = ap.parse_args()

    clips = list(load_clips(args.dataset))
    print(f"Clips leídos: {len(clips)}  ·  proporción {args.aspect:.2f}")

    per_word = defaultdict(list)
    skipped = 0
    for signer, word, data, mask in clips:
        ref = clip_reference(data, mask, args.aspect)
        if ref is None:
            skipped += 1
            continue
        per_word[word].append((signer, *ref))
    print(f"Clips descartados: {skipped}\n")

    out, total_refs = [], 0
    for folder, refs in sorted(per_word.items()):
        word = DISPLAY.get(folder, folder)
        hand_count = Counter(r[1] for r in refs).most_common(1)[0][0]
        refs = [r for r in refs if r[1] == hand_count]

        by_signer = defaultdict(list)
        for r in refs:
            by_signer[r[0]].append(r)

        references = []
        for signer, rs in sorted(by_signer.items()):
            if len(rs) < MIN_CLIPS_PER_SIGNER:
                continue
            references.append({
                "signer": signer,
                "clips": len(rs),
                "distances": rounded_mean([r[2] for r in rs]),
                "angles": rounded_mean([r[3] for r in rs]),
                "normalizedLandmarks": rounded_mean([r[4] for r in rs]),
                "sampleCount": 5,  # revisar el CHECK de num_muestras
            })
        if not references:
            print(f"  {word:8s} ✗ sin referencias suficientes")
            continue

        out.append({
            "word": word,
            "category": CATEGORY.get(word, "Respuestas"),
            "description": f"Corpus LSM de 22 señas (Zenodo 23744039, CC BY 4.0): "
                           f"{len(references)} intérpretes, {sum(r['clips'] for r in references)} clips",
            "handCount": hand_count,
            "references": references,
        })
        total_refs += len(references)
        print(f"  {word:8s} manos={hand_count}  referencias={len(references)}  categoría={out[-1]['category']}")

    args.salida.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"\n{len(out)} señas, {total_refs} referencias → {args.salida}")


if __name__ == "__main__":
    main()