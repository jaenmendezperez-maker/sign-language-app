import { Component, computed, inject, OnInit, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { CatalogApiService } from '../../core/services/catalog-api.service';
import { Category, Sign } from '../../models/catalog.model';
import { averageFeatures, HandFeatures } from '../../recognition/hand-features';
import { SignDetector } from '../../recognition/sign-detector';
import { CameraView, HandsFrame } from '../../shared/components/camera-view/camera-view';
import { HandSketch } from '../../shared/components/hand-sketch';

const SAMPLES = 5; // RF-11: al menos 5 muestras

type CaptureState = 'idle' | 'capturing' | 'saving';

/** CU-13 Registrar referencia geométrica de una seña (solo administrador). */
@Component({
  selector: 'app-register-sign',
  imports: [FormsModule, CameraView, HandSketch],
  templateUrl: './register-sign.html',
})
export class RegisterSign implements OnInit {
  private readonly api = inject(CatalogApiService);
  private readonly camera = viewChild.required(CameraView);

  protected readonly required = SAMPLES;
  protected readonly categories = signal<Category[]>([]);
  protected readonly state = signal<CaptureState>('idle');
  protected readonly samples = signal<HandFeatures[]>([]);
  protected readonly handsInView = signal(0);
  protected readonly error = signal<string | null>(null);
  protected readonly saved = signal<Sign | null>(null);

  protected word = '';
  protected categoryId: number | null = null;
  protected visualUrl = '';

  private readonly detector = new SignDetector({ stableFrames: 10 });

  async ngOnInit(): Promise<void> {
    try {
      this.categories.set(await this.api.categories());
    } catch (err) {
      this.error.set((err as Error).message);
    }
  }

  protected readonly cameraOn = computed(() => this.camera().running());

  /** Para practicar la postura y ver los puntos antes de capturar. */
  protected async toggleCamera(): Promise<void> {
    if (this.cameraOn()) this.camera().stop();
    else await this.camera().start();
  }

  protected async startCapture(): Promise<void> {
    this.error.set(null);
    this.saved.set(null);
    if (!this.word.trim()) {
      this.error.set('Falta la palabra: escribe qué significa la seña (por ejemplo «hola»).');
      return;
    }
    if (this.categoryId === null) {
      this.error.set('Elige una categoría.');
      return;
    }
    if (!(await this.camera().start())) return;
    this.samples.set([]);
    this.detector.reset();
    this.state.set('capturing');
  }

  protected cancel(): void {
    this.state.set('idle');
    this.samples.set([]);
  }

  protected onFrame(frame: HandsFrame): void {
    this.handsInView.set(frame.hands.length);
    if (this.state() !== 'capturing') return;

    const sample = this.detector.push(frame.hands);
    if (!sample) return;

    const first = this.samples()[0];
    if (first && first.handCount !== sample.handCount) {
      // Todas las muestras deben tener el mismo número de manos: se rechaza y se empieza de nuevo
      this.error.set('Las muestras no tienen el mismo número de manos. Se reinició la captura.');
      this.samples.set([]);
      return;
    }

    this.samples.update((s) => [...s, sample]);
    if (this.samples().length >= SAMPLES) void this.save();
  }

  private async save(): Promise<void> {
    this.state.set('saving');
    try {
      const avg = averageFeatures(this.samples());
      const sign = await this.api.registerReference({
        word: this.word.trim(),
        categoryId: this.categoryId!,
        handCount: avg.handCount,
        visualUrl: this.visualUrl.trim() || null,
        distances: avg.distances,
        angles: avg.angles,
        normalizedLandmarks: avg.normalized,
        sampleCount: this.samples().length,
      });
      this.saved.set(sign);
      this.word = '';
      this.visualUrl = '';
      this.camera().stop();
    } catch (err) {
      this.error.set((err as Error).message);
    } finally {
      this.samples.set([]);
      this.state.set('idle');
    }
  }
}
