import { Component, computed, input } from '@angular/core';

import { COORDINATES_PER_HAND, HAND_CONNECTIONS } from '../../recognition/hand-geometry';

/**
 * Dibuja la mano de una referencia a partir de sus puntos normalizados (RF-10).
 * Así el catálogo tiene una referencia visual sin guardar imágenes de nadie.
 */
@Component({
  selector: 'app-hand-sketch',
  template: `
    <svg [attr.viewBox]="view().box" role="img" [attr.aria-label]="label()" class="hand-sketch">
      @for (line of view().lines; track $index) {
        <line [attr.x1]="line[0]" [attr.y1]="line[1]" [attr.x2]="line[2]" [attr.y2]="line[3]" />
      }
      @for (p of view().points; track $index) {
        <circle [attr.cx]="p[0]" [attr.cy]="p[1]" r="0.06" />
      }
    </svg>
  `,
})
export class HandSketch {
  /** 63 valores por mano (x, y, z), como los guarda REFERENCIA_GEOMETRICA. */
  readonly landmarks = input.required<number[]>();
  readonly label = input('Dibujo de la mano');

  protected readonly view = computed(() => {
    const values = this.landmarks();
    const hands = Math.max(1, Math.floor(values.length / COORDINATES_PER_HAND));
    const points: [number, number][] = [];
    for (let h = 0; h < hands; h++) {
      for (let i = 0; i < 21; i++) {
        const o = h * COORDINATES_PER_HAND + i * 3;
        points.push([values[o] + h * 3, values[o + 1]]); // dos manos lado a lado
      }
    }
    const xs = points.map((p) => p[0]);
    const ys = points.map((p) => p[1]);
    const [minX, maxX, minY, maxY] = [Math.min(...xs), Math.max(...xs), Math.min(...ys), Math.max(...ys)];
    const pad = 0.3;
    const lines = Array.from({ length: hands }).flatMap((_, h) =>
      HAND_CONNECTIONS.map(([a, b]) => [...points[h * 21 + a], ...points[h * 21 + b]]),
    );
    return {
      box: `${minX - pad} ${minY - pad} ${maxX - minX + 2 * pad} ${maxY - minY + 2 * pad}`,
      lines,
      points,
    };
  });
}
