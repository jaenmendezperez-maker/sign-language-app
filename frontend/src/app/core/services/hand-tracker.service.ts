import { Injectable } from '@angular/core';
import type { HandLandmarker } from '@mediapipe/tasks-vision';

import { DetectedHand, Handedness } from '../../recognition/hand-features';
import { HAND_CONNECTIONS } from '../../recognition/hand-geometry';

/** El wasm se copia desde node_modules en angular.json, así siempre coincide con la versión instalada. */
const WASM_PATH = '/mediapipe/wasm';
const HAND_MODEL =
  'https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task';

/**
 * MediaPipe Hands en el navegador (RF-02): 21 puntos por mano, una o dos manos.
 * El video nunca sale del equipo (RNF-06).
 */
@Injectable({ providedIn: 'root' })
export class HandTrackerService {
  private landmarker: HandLandmarker | null = null;
  private loading: Promise<void> | null = null;

  /** Carga el modelo una sola vez (la librería se importa de forma diferida). */
  init(): Promise<void> {
    this.loading ??= this.load().catch((err) => {
      this.loading = null;
      throw err;
    });
    return this.loading;
  }

  /** `timestampMs` debe ir siempre en aumento. */
  detect(video: HTMLVideoElement, timestampMs: number): DetectedHand[] {
    if (!this.landmarker) throw new Error('El modelo de MediaPipe Hands no se ha cargado.');
    const result = this.landmarker.detectForVideo(video, timestampMs);
    return result.landmarks.map((landmarks, i) => ({
      side: (result.handedness[i]?.[0]?.categoryName ?? 'Right') as Handedness,
      landmarks,
      world: result.worldLandmarks[i],
    }));
  }

  /** Dibuja los 21 puntos y sus conexiones sobre el canvas (criterio de RF-02). */
  draw(canvas: HTMLCanvasElement, hands: DetectedHand[]): void {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    const { width: w, height: h } = canvas;

    for (const hand of hands) {
      ctx.strokeStyle = hand.side === 'Left' ? '#4fc3f7' : '#ffd54f';
      ctx.lineWidth = Math.max(2, w / 320);
      for (const [a, b] of HAND_CONNECTIONS) {
        ctx.beginPath();
        ctx.moveTo(hand.landmarks[a].x * w, hand.landmarks[a].y * h);
        ctx.lineTo(hand.landmarks[b].x * w, hand.landmarks[b].y * h);
        ctx.stroke();
      }
      ctx.fillStyle = '#ffffff';
      for (const p of hand.landmarks) {
        ctx.beginPath();
        ctx.arc(p.x * w, p.y * h, Math.max(2.5, w / 260), 0, Math.PI * 2);
        ctx.fill();
      }
    }
  }

  private async load(): Promise<void> {
    const { FilesetResolver, HandLandmarker } = await import('@mediapipe/tasks-vision');
    const fileset = await FilesetResolver.forVisionTasks(WASM_PATH);
    this.landmarker = await HandLandmarker.createFromOptions(fileset, {
      baseOptions: { modelAssetPath: HAND_MODEL, delegate: 'GPU' },
      runningMode: 'VIDEO',
      numHands: 2,
    });
  }
}
