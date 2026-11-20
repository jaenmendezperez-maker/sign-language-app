import { Component, computed, input } from '@angular/core';

import { COORDINATES_PER_HAND, FINGER_CHAINS, LANDMARKS_PER_HAND, MIDDLE_MCP, WRIST } from '../../recognition/hand-geometry';

/**
 * Dibuja la mano de una referencia a partir de sus puntos normalizados (RF-10).
 * Así el catálogo tiene una referencia visual sin guardar imágenes de nadie.
 *
 * Se dibuja en 2D, rellena y con color de piel, en la misma orientación con que se capturó
 * la seña (vista de frente, como la ve la cámara).
 */

type P2 = [number, number];

interface HandShapes {
  palm: string;
  forearm: string;
  fingers: { points: string; width: number }[];
}

const INDEX_MCP = 5;
const PINKY_MCP = 17;
/** Contorno de la palma: muñeca, base del pulgar y bases de los cuatro dedos. */
const PALM = [WRIST, 1, 2, INDEX_MCP, MIDDLE_MCP, 13, PINKY_MCP];
/** Grosores en unidades de "muñeca → base del dedo medio". */
const FINGER_WIDTH = 0.17;
const THUMB_WIDTH = 0.21;
const FOREARM_WIDTH = 0.32;
const OUTLINE = 0.035;

const toSvg = (pts: P2[]): string => pts.map(([x, y]) => `${x.toFixed(3)},${y.toFixed(3)}`).join(' ');

@Component({
  selector: 'app-hand-sketch',
  template: `
    <svg [attr.viewBox]="view().box" role="img" [attr.aria-label]="label()" class="hand-sketch">
      <!-- 1) Contorno -->
      @for (hand of view().hands; track $index) {
        <polyline class="outline line" [attr.points]="hand.forearm" [attr.stroke-width]="forearm + outline * 2" />
        <polygon class="outline" [attr.points]="hand.palm" [attr.stroke-width]="palmStroke + outline * 2" />
        @for (f of hand.fingers; track $index) {
          <polyline class="outline line" [attr.points]="f.points" [attr.stroke-width]="f.width + outline * 2" />
        }
      }
      <!-- 2) Piel: tapa los contornos interiores y deja solo la silueta -->
      @for (hand of view().hands; track $index) {
        <polyline class="skin line" [attr.points]="hand.forearm" [attr.stroke-width]="forearm" />
        <polygon class="skin" [attr.points]="hand.palm" [attr.stroke-width]="palmStroke" />
        @for (f of hand.fingers; track $index) {
          <polyline class="skin line" [attr.points]="f.points" [attr.stroke-width]="f.width" />
        }
      }
      <!-- 3) Líneas finas para distinguir los dedos cuando se juntan -->
      @for (hand of view().hands; track $index) {
        @for (f of hand.fingers; track $index) {
          <polyline class="crease" [attr.points]="f.points" />
        }
      }
    </svg>
  `,
  styles: `
    .hand-sketch { display: block; width: 100%; height: 100%; }
    .outline, .skin { stroke-linecap: round; stroke-linejoin: round; }
    .outline { fill: var(--hand-outline, #9c6b52); stroke: var(--hand-outline, #9c6b52); }
    .skin { fill: var(--hand-skin, #e6b293); stroke: var(--hand-skin, #e6b293); }
    .line { fill: none; }
    .crease {
      fill: none;
      stroke: var(--hand-crease, #c38b6e);
      stroke-width: 0.022;
      stroke-linecap: round;
      stroke-linejoin: round;
    }
  `,
})
export class HandSketch {
  /** 63 valores por mano (x, y, z), como los guarda REFERENCIA_GEOMETRICA. */
  readonly landmarks = input.required<number[]>();
  readonly label = input('Dibujo de la mano');

  protected readonly outline = OUTLINE;
  protected readonly forearm = FOREARM_WIDTH;
  /** El trazo de la palma redondea sus esquinas. */
  protected readonly palmStroke = FINGER_WIDTH;

  protected readonly view = computed(() => {
    const values = this.landmarks();
    const handCount = Math.max(1, Math.min(2, Math.floor(values.length / COORDINATES_PER_HAND)));

    const all: P2[] = [];
    const hands: HandShapes[] = [];
    let cursor = 0; // borde derecho de la mano anterior

    for (let h = 0; h < handCount; h++) {
      let pts: P2[] = Array.from({ length: LANDMARKS_PER_HAND }, (_, i) => {
        const o = h * COORDINATES_PER_HAND + i * 3;
        return [values[o] ?? 0, values[o + 1] ?? 0];
      });

      // Dos manos lado a lado, sin encimarse
      const minX = Math.min(...pts.map((p) => p[0]));
      const maxX = Math.max(...pts.map((p) => p[0]));
      const shift = h === 0 ? 0 : cursor + 0.7 - minX;
      pts = pts.map(([x, y]) => [x + shift, y]);
      cursor = maxX + shift;

      // Antebrazo: sigue la dirección base del dedo medio → muñeca
      const [wx, wy] = pts[WRIST];
      const [mx, my] = pts[MIDDLE_MCP];
      const len = Math.hypot(wx - mx, wy - my) || 1;
      const forearmEnd: P2 = [wx + ((wx - mx) / len) * 0.45, wy + ((wy - my) / len) * 0.45];

      all.push(...pts, forearmEnd);
      hands.push({
        palm: toSvg(PALM.map((i) => pts[i])),
        forearm: toSvg([pts[WRIST], forearmEnd]),
        fingers: FINGER_CHAINS.map((chain, f) => ({
          points: toSvg(chain.slice(1).map((i) => pts[i])), // de la base a la punta
          width: f === 0 ? THUMB_WIDTH : FINGER_WIDTH,
        })),
      });
    }

    const xs = all.map((p) => p[0]);
    const ys = all.map((p) => p[1]);
    const pad = 0.3;
    const [minX, maxX, minY, maxY] = [Math.min(...xs), Math.max(...xs), Math.min(...ys), Math.max(...ys)];
    // Cuadro cuadrado para que todas las tarjetas tengan la mano a la misma escala visual
    const side = Math.max(maxX - minX, maxY - minY) + 2 * pad;
    const cx = (minX + maxX) / 2;
    const cy = (minY + maxY) / 2;

    return { box: `${cx - side / 2} ${cy - side / 2} ${side} ${side}`, hands };
  });
}