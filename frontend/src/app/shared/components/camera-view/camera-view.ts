import { Component, ElementRef, inject, OnDestroy, OnInit, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { HandTrackerService } from '../../../core/services/hand-tracker.service';
import { DetectedHand } from '../../../recognition/hand-features';

export interface HandsFrame {
  hands: DetectedHand[];
  timestamp: number;
}

const DEVICE_KEY = 'signlink.camera';
const FPS_LOG_INTERVAL_MS = 10_000;

/**
 * Cámara + MediaPipe Hands. Emite las manos detectadas en cada cuadro y dibuja los
 * landmarks sobre la vista previa (RF-01, RF-02). Mide los fps (RNF-02).
 */
@Component({
  selector: 'app-camera-view',
  imports: [FormsModule],
  templateUrl: './camera-view.html',
})
export class CameraView implements OnInit, OnDestroy {
  readonly frame = output<HandsFrame>();

  private readonly tracker = inject(HandTrackerService);
  private readonly video = viewChild.required<ElementRef<HTMLVideoElement>>('video');
  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');

  protected readonly devices = signal<MediaDeviceInfo[]>([]);
  protected readonly selectedId = signal<string>(this.restoreDevice());
  readonly running = signal(false);
  readonly loading = signal(false);
  readonly fps = signal(0);
  readonly error = signal<string | null>(null);

  private stream: MediaStream | null = null;
  private rafId: number | null = null;
  private lastVideoTime = -1;
  private frames = 0;
  private fpsWindowStart = 0;
  private lastFpsLog = 0;

  ngOnInit(): void {
    void this.refreshDevices();
  }

  /** Pide permiso, abre la cámara elegida y empieza a detectar. Devuelve false si no se pudo. */
  async start(): Promise<boolean> {
    if (this.running()) return true;
    this.error.set(null);
    this.loading.set(true);
    try {
      const [, stream] = await Promise.all([this.tracker.init(), this.openCamera()]);
      this.stream = stream;
      const video = this.video().nativeElement;
      video.srcObject = stream;
      await video.play();

      const settings = stream.getVideoTracks()[0]?.getSettings();
      if (settings?.deviceId) this.rememberDevice(settings.deviceId);
      await this.refreshDevices(); // las etiquetas solo aparecen después del permiso

      this.running.set(true);
      this.fpsWindowStart = this.lastFpsLog = performance.now();
      this.rafId = requestAnimationFrame(this.loop);
      return true;
    } catch (err) {
      this.stop();
      this.error.set(cameraErrorMessage(err));
      return false;
    } finally {
      this.loading.set(false);
    }
  }

  /** Detiene la captura y libera la cámara. */
  stop(): void {
    if (this.rafId !== null) cancelAnimationFrame(this.rafId);
    this.rafId = null;
    this.stream?.getTracks().forEach((t) => t.stop());
    this.stream = null;
    const video = this.video().nativeElement;
    video.srcObject = null;
    const canvas = this.canvas().nativeElement;
    canvas.getContext('2d')?.clearRect(0, 0, canvas.width, canvas.height);
    this.lastVideoTime = -1;
    this.running.set(false);
    this.fps.set(0);
  }

  async selectDevice(id: string): Promise<void> {
    this.rememberDevice(id);
    if (this.running()) {
      this.stop();
      await this.start();
    }
  }

  ngOnDestroy(): void {
    this.stop();
  }

  private readonly loop = (): void => {
    if (!this.running()) return;
    const video = this.video().nativeElement;

    if (video.readyState >= HTMLMediaElement.HAVE_CURRENT_DATA && video.currentTime !== this.lastVideoTime) {
      this.lastVideoTime = video.currentTime;
      const now = performance.now();
      try {
        const hands = this.tracker.detect(video, now);
        this.syncCanvasSize(video);
        this.tracker.draw(this.canvas().nativeElement, hands);
        this.countFrame(now);
        this.frame.emit({ hands, timestamp: now });
      } catch (err) {
        this.stop();
        this.error.set(`Falló la detección de manos: ${(err as Error).message}`);
        return;
      }
    }
    this.rafId = requestAnimationFrame(this.loop);
  };

  private async openCamera(): Promise<MediaStream> {
    const id = this.selectedId();
    // 720p a 30 fps (RNF-05); si la cámara no llega, el navegador da lo más cercano
    const video: MediaTrackConstraints = { width: { ideal: 1280 }, height: { ideal: 720 }, frameRate: { ideal: 30 } };
    if (id) video.deviceId = { exact: id };
    try {
      return await navigator.mediaDevices.getUserMedia({ video, audio: false });
    } catch (err) {
      // La cámara guardada ya no existe: usa la predeterminada
      if (id && err instanceof DOMException && (err.name === 'OverconstrainedError' || err.name === 'NotFoundError')) {
        this.rememberDevice('');
        delete video.deviceId;
        return navigator.mediaDevices.getUserMedia({ video, audio: false });
      }
      throw err;
    }
  }

  private async refreshDevices(): Promise<void> {
    if (!navigator.mediaDevices?.enumerateDevices) return;
    const all = await navigator.mediaDevices.enumerateDevices();
    this.devices.set(all.filter((d) => d.kind === 'videoinput'));
  }

  private syncCanvasSize(video: HTMLVideoElement): void {
    const canvas = this.canvas().nativeElement;
    if (canvas.width !== video.videoWidth || canvas.height !== video.videoHeight) {
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;
    }
  }

  /** fps procesados; se registran en consola cada 10 s para verificar RNF-02 (≥ 15 fps). */
  private countFrame(now: number): void {
    this.frames++;
    if (now - this.fpsWindowStart >= 1000) {
      this.fps.set(Math.round((this.frames * 1000) / (now - this.fpsWindowStart)));
      this.frames = 0;
      this.fpsWindowStart = now;
    }
    if (now - this.lastFpsLog >= FPS_LOG_INTERVAL_MS) {
      console.info(`[SignLink] fps procesados: ${this.fps()}`);
      this.lastFpsLog = now;
    }
  }

  private rememberDevice(id: string): void {
    this.selectedId.set(id);
    try {
      localStorage.setItem(DEVICE_KEY, id);
    } catch {
      /* preferencia solo para esta pestaña */
    }
  }

  private restoreDevice(): string {
    try {
      return localStorage.getItem(DEVICE_KEY) ?? '';
    } catch {
      return '';
    }
  }
}

function cameraErrorMessage(err: unknown): string {
  const name = err instanceof DOMException ? err.name : '';
  if (name === 'NotAllowedError')
    return 'Se negó el permiso para usar la cámara. Habilítalo desde el icono de cámara en la barra de direcciones del navegador y vuelve a intentarlo.';
  if (name === 'NotFoundError') return 'No se encontró ninguna cámara.';
  if (name === 'NotReadableError') return 'La cámara está siendo usada por otra aplicación. Ciérrala o elige otra cámara.';
  return `No se pudo iniciar la cámara o cargar MediaPipe: ${err instanceof Error ? err.message : String(err)}`;
}
