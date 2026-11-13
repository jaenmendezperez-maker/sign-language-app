import { averageFeatures, DetectedHand, handFeatures, HandFeatures } from './hand-features';

export interface DetectorOptions {
  /** Cuadros seguidos con la mano quieta para dar la seña por completada. */
  stableFrames: number;
  /** Movimiento medio por cuadro (coordenadas de imagen 0–1) por debajo del cual la mano está quieta. */
  stillThreshold: number;
  /** Movimiento a partir del cual se considera que empezó otra seña. */
  moveThreshold: number;
}

export const DEFAULT_DETECTOR: DetectorOptions = { stableFrames: 8, stillThreshold: 0.01, moveThreshold: 0.025 };

/**
 * Decide cuándo una seña está "completada": la mano (o manos) se mantiene quieta durante
 * `stableFrames` cuadros. Entonces devuelve el promedio de esos cuadros una sola vez y no
 * vuelve a emitir hasta que la mano se mueva o salga del encuadre.
 */
export class SignDetector {
  private readonly options: DetectorOptions;
  private previous: number[] | null = null;
  private window: HandFeatures[] = [];
  private locked = false;

  constructor(options: Partial<DetectorOptions> = {}) {
    this.options = { ...DEFAULT_DETECTOR, ...options };
  }

  /** Procesa un cuadro; devuelve las características de la seña cuando se completa. */
  push(hands: DetectedHand[]): HandFeatures | null {
    const features = handFeatures(hands);
    if (!features) {
      this.reset();
      return null;
    }

    const raw = hands.flatMap((h) => h.landmarks.flatMap((p) => [p.x, p.y]));
    const motion = this.previous && this.previous.length === raw.length ? meanDisplacement(this.previous, raw) : Infinity;
    this.previous = raw;

    if (motion === Infinity) {
      // Primer cuadro o cambió el número de manos: todavía no se puede medir movimiento
      this.window = [];
      this.locked = false;
      return null;
    }

    if (motion > this.options.moveThreshold) {
      this.locked = false;
      this.window = [];
      return null;
    }

    if (motion > this.options.stillThreshold) {
      this.window = [];
      return null;
    }

    if (this.locked) return null;

    this.window.push(features);
    if (this.window.length < this.options.stableFrames) return null;

    const completed = averageFeatures(this.window);
    this.window = [];
    this.locked = true;
    return completed;
  }

  reset(): void {
    this.previous = null;
    this.window = [];
    this.locked = false;
  }
}

function meanDisplacement(a: number[], b: number[]): number {
  let sum = 0;
  for (let i = 0; i < a.length; i += 2) sum += Math.hypot(a[i] - b[i], a[i + 1] - b[i + 1]);
  return sum / (a.length / 2);
}
