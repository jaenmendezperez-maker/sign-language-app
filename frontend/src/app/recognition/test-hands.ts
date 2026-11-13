import { DetectedHand, Handedness, Point3 } from './hand-features';
import { FINGER_CHAINS } from './hand-geometry';

/**
 * Mano sintética para pruebas: cada dedo sale de la palma con una flexión dada (0 = recto,
 * 90 = muy doblado). Permite crear "señas" distintas sin una cámara.
 */
export function syntheticHand(flex: number[] = [0, 0, 0, 0, 0], opts: { scale?: number; dx?: number; dy?: number } = {}): Point3[] {
  const { scale = 0.1, dx = 0.5, dy = 0.6 } = opts;
  const pts: Point3[] = Array.from({ length: 21 }, () => ({ x: 0, y: 0, z: 0 }));
  const baseAngles = [-60, -20, 0, 20, 40]; // dirección de cada dedo desde la muñeca (grados)

  FINGER_CHAINS.forEach((chain, f) => {
    let angle = (baseAngles[f] - 90) * (Math.PI / 180); // hacia arriba en la imagen
    let x = 0;
    let y = 0;
    chain.slice(1).forEach((idx, seg) => {
      const len = seg === 0 ? 1 : 0.45;
      if (seg > 0) angle += (flex[f] * Math.PI) / 180;
      x += Math.cos(angle) * len;
      y += Math.sin(angle) * len;
      pts[idx] = { x, y, z: seg * 0.01 };
    });
  });
  return pts.map((p) => ({ x: dx + p.x * scale, y: dy + p.y * scale, z: p.z * scale }));
}

export const hand = (landmarks: Point3[], side: Handedness = 'Right'): DetectedHand => ({ side, landmarks });

/** Refleja la mano horizontalmente (la misma seña hecha con la otra mano). */
export const mirror = (pts: Point3[]): Point3[] => pts.map((p) => ({ ...p, x: 1 - p.x }));
