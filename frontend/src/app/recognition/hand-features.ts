import {
  ANGLES_PER_HAND,
  DISTANCE_PAIRS,
  DISTANCES_PER_HAND,
  FINGER_CHAINS,
  LANDMARKS_PER_HAND,
  MIDDLE_MCP,
  WRIST,
} from './hand-geometry';

export interface Point3 {
  x: number;
  y: number;
  z: number;
}

export type Handedness = 'Left' | 'Right';

export interface DetectedHand {
  side: Handedness;
  /** 21 puntos en coordenadas de imagen (0–1): sirven para dibujar y medir movimiento. */
  landmarks: Point3[];
  /**
   * 21 puntos en metros (worldLandmarks de MediaPipe). Son los que se usan para la geometría:
   * las coordenadas de imagen se normalizan por separado con el ancho y el alto, así que en una
   * cámara 16:9 deforman las distancias y los ángulos.
   */
  world?: Point3[];
}

/** Vector geométrico de una o dos manos (para dos manos: primero la izquierda). */
export interface HandFeatures {
  handCount: 1 | 2;
  distances: number[];
  angles: number[];
  /** Puntos normalizados aplanados (x, y, z por punto). */
  normalized: number[];
}

const sub = (a: Point3, b: Point3): Point3 => ({ x: a.x - b.x, y: a.y - b.y, z: a.z - b.z });
const norm = (v: Point3): number => Math.hypot(v.x, v.y, v.z);
const dist = (a: Point3, b: Point3): number => norm(sub(a, b));

/** Ángulo en grados en el vértice `b` formado por a-b-c (180 = dedo recto). */
export function angleAt(a: Point3, b: Point3, c: Point3): number {
  const u = sub(a, b);
  const v = sub(c, b);
  const denom = norm(u) * norm(v);
  if (denom < 1e-9) return 0;
  const cos = Math.min(1, Math.max(-1, (u.x * v.x + u.y * v.y + u.z * v.z) / denom));
  return (Math.acos(cos) * 180) / Math.PI;
}

/**
 * Centra la mano en la muñeca y la escala por la distancia muñeca → base del dedo medio.
 * Así el resultado no depende de dónde esté la mano en el encuadre ni de qué tan cerca
 * esté de la cámara (criterio de aceptación de RF-03).
 */
export function normalizeHand(landmarks: Point3[]): Point3[] {
  if (landmarks.length !== LANDMARKS_PER_HAND) {
    throw new Error(`Se esperaban ${LANDMARKS_PER_HAND} puntos por mano, llegaron ${landmarks.length}.`);
  }
  const origin = landmarks[WRIST];
  const scale = dist(landmarks[MIDDLE_MCP], origin) || 1;
  return landmarks.map((p) => ({ x: (p.x - origin.x) / scale, y: (p.y - origin.y) / scale, z: (p.z - origin.z) / scale }));
}

/** 12 distancias y 15 ángulos de una mano. Ambos son invariantes a rotación y reflejo. */
export function singleHandFeatures(landmarks: Point3[]): { distances: number[]; angles: number[]; normalized: number[] } {
  const p = normalizeHand(landmarks);
  const distances = DISTANCE_PAIRS.map(([a, b]) => dist(p[a], p[b]));
  const angles = FINGER_CHAINS.flatMap((chain) => [1, 2, 3].map((i) => angleAt(p[chain[i - 1]], p[chain[i]], p[chain[i + 1]])));
  const normalized = p.flatMap((q) => [q.x, q.y, q.z]);

  if (distances.length !== DISTANCES_PER_HAND || angles.length !== ANGLES_PER_HAND) {
    throw new Error('Tamaño de vector inesperado');
  }
  return { distances, angles, normalized };
}

/** Vector de la(s) mano(s) detectada(s): izquierda primero, luego derecha. */
export function handFeatures(hands: DetectedHand[]): HandFeatures | null {
  if (hands.length === 0) return null;
  const ordered = [...hands].sort((a, b) => (a.side === b.side ? 0 : a.side === 'Left' ? -1 : 1)).slice(0, 2);
  const parts = ordered.map((h) => singleHandFeatures(h.world ?? h.landmarks));
  return {
    handCount: ordered.length as 1 | 2,
    distances: parts.flatMap((f) => f.distances),
    angles: parts.flatMap((f) => f.angles),
    normalized: parts.flatMap((f) => f.normalized),
  };
}

/** Promedio componente a componente (para las muestras de una seña estable). */
export function averageFeatures(samples: HandFeatures[]): HandFeatures {
  if (samples.length === 0) throw new Error('No hay muestras para promediar.');
  const handCount = samples[0].handCount;
  if (samples.some((s) => s.handCount !== handCount)) throw new Error('Las muestras tienen distinto número de manos.');

  const avg = (key: 'distances' | 'angles' | 'normalized') =>
    samples[0][key].map((_, i) => samples.reduce((sum, s) => sum + s[key][i], 0) / samples.length);

  return { handCount, distances: avg('distances'), angles: avg('angles'), normalized: avg('normalized') };
}
