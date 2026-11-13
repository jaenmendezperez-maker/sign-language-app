import { Sign } from '../models/catalog.model';
import { HandFeatures } from './hand-features';

/**
 * Tolerancias del algoritmo de comparación (RF-03). Un error cuadrático medio (RMS) igual a
 * la tolerancia deja el puntaje en 0. Se usa RMS y no el promedio simple para que una seña que
 * difiere en un solo dedo (puño vs. pulgar arriba) no salga casi igual. Se ajustan con las pruebas de precisión (RNF-03) sin tocar el
 * resto del código.
 */
export const SIMILARITY = {
  /** Error RMS de distancia (en unidades de "muñeca → base del dedo medio") que da puntaje 0. */
  distanceTolerance: 0.6,
  /** Error RMS de ángulo (grados) que da puntaje 0. */
  angleToleranceDeg: 45,
  distanceWeight: 0.5,
  angleWeight: 0.5,
} as const;

interface Comparable {
  distances: number[];
  angles: number[];
}

const rms = (a: number[], b: number[]): number =>
  Math.sqrt(a.reduce((sum, v, i) => sum + (v - b[i]) ** 2, 0) / Math.max(a.length, 1));

/** Puntaje de similitud de 0 a 100 entre la seña detectada y una referencia. */
export function similarity(detected: Comparable, reference: Comparable): number {
  if (detected.distances.length !== reference.distances.length || detected.angles.length !== reference.angles.length) {
    return 0;
  }
  const distanceError = rms(detected.distances, reference.distances) / SIMILARITY.distanceTolerance;
  const angleError = rms(detected.angles, reference.angles) / SIMILARITY.angleToleranceDeg;
  const error = SIMILARITY.distanceWeight * distanceError + SIMILARITY.angleWeight * angleError;
  return Math.round(Math.max(0, 1 - error) * 10000) / 100; // 2 decimales
}

export interface Match {
  sign: Sign | null;
  score: number;
}

/**
 * La seña reconocida es la referencia con mayor puntaje (RF-03). Solo se compara contra
 * señas con el mismo número de manos. Una seña puede tener varias referencias (variantes).
 */
export function bestMatch(detected: HandFeatures, catalog: readonly Sign[]): Match {
  let best: Match = { sign: null, score: 0 };
  for (const sign of catalog) {
    if (sign.handCount !== detected.handCount) continue;
    for (const ref of sign.references) {
      const score = similarity(detected, ref);
      if (score > best.score) best = { sign, score };
    }
  }
  return best;
}
