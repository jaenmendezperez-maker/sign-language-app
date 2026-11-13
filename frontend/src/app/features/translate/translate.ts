import { DatePipe } from '@angular/common';
import { Component, computed, effect, ElementRef, inject, OnDestroy, OnInit, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ConsultationApiService } from '../../core/services/consultation-api.service';
import { SpeechService } from '../../core/services/speech.service';
import { Sign } from '../../models/catalog.model';
import { CertaintyConfig, Consultation, IN_PHRASE, RecordStatus, TranslationRecord } from '../../models/consultation.model';
import { DEFAULT_CERTAINTY, levelFor } from '../../recognition/certainty';
import { handFeatures, HandFeatures } from '../../recognition/hand-features';
import { bestMatch } from '../../recognition/similarity';
import { SignDetector } from '../../recognition/sign-detector';
import { CameraView, HandsFrame } from '../../shared/components/camera-view/camera-view';
import { CertaintyBadge } from '../../shared/components/certainty-badge';
import { ScorePipe } from '../../shared/pipes/score.pipe';

type SessionState = 'idle' | 'starting' | 'active' | 'paused' | 'finished';

/** Resultado mostrado al instante, antes de que responda el servidor (RNF-01). */
interface LiveResult {
  record: TranslationRecord | null;
  word: string | null;
  level: TranslationRecord['certaintyLevel'];
  score: number;
}

const NO_HANDS_MS = 3000; // RF-14
const STATE_LABELS: Record<SessionState, string> = {
  idle: 'Sin iniciar',
  starting: 'Iniciando…',
  active: 'Activa',
  paused: 'En pausa',
  finished: 'Finalizada',
};
const STATUS_LABELS: Partial<Record<RecordStatus, string>> = {
  Pending: 'pendiente',
  Confirmed: 'confirmada',
  NotConfirmed: 'no confirmada',
  Discarded: 'descartada',
};

/** Pantalla principal del médico: CU-02 a CU-11. */
@Component({
  selector: 'app-translate',
  imports: [CameraView, CertaintyBadge, ScorePipe, DatePipe, RouterLink],
  templateUrl: './translate.html',
})
export class Translate implements OnInit, OnDestroy {
  private readonly api = inject(ConsultationApiService);
  private readonly catalogApi = inject(CatalogApiService);
  protected readonly speech = inject(SpeechService);
  private readonly camera = viewChild.required(CameraView);
  private readonly historyList = viewChild<ElementRef<HTMLElement>>('historyList');

  protected readonly catalog = signal<Sign[]>([]);
  protected readonly catalogLoaded = signal(false);
  protected readonly config = signal<CertaintyConfig>(DEFAULT_CERTAINTY);
  protected readonly state = signal<SessionState>('idle');
  protected readonly consultation = signal<Consultation | null>(null);
  protected readonly records = signal<TranslationRecord[]>([]);
  protected readonly live = signal<LiveResult | null>(null);
  protected readonly repeat = signal(false);
  protected readonly noHands = signal(false);
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly confirmingFinish = signal(false);
  protected readonly confirmingClear = signal(false);
  protected readonly exportMessage = signal<string | null>(null);
  /** Lo que el sistema ve en este momento (se actualiza unas veces por segundo). */
  protected readonly preview = signal<{ word: string | null; score: number; level: TranslationRecord['certaintyLevel'] } | null>(null);
  /** Segundos restantes para confirmar cada palabra pendiente. */
  protected readonly countdown = signal<Partial<Record<string, number>>>({});

  protected readonly stateLabel = computed(() => STATE_LABELS[this.state()]);
  protected readonly statusLabels = STATUS_LABELS;
  protected readonly phrase = computed(() =>
    this.records()
      .filter((r) => IN_PHRASE.has(r.status))
      .map((r) => r.word)
      .join(' '),
  );

  private readonly detector = new SignDetector();
  private readonly pendingTimers = new Map<string, ReturnType<typeof setInterval>>();
  private noHandsTimer: ReturnType<typeof setInterval> | null = null;
  private lastHandAt = 0;
  private frameCount = 0;

  constructor() {
    // El historial muestra lo más reciente al final: desplaza hasta abajo al agregar (RF-09)
    effect(() => {
      this.records();
      const el = this.historyList()?.nativeElement;
      if (el) queueMicrotask(() => (el.scrollTop = el.scrollHeight));
    });
  }

  async ngOnInit(): Promise<void> {
    try {
      const [config, catalog] = await Promise.all([this.api.certainty().catch(() => DEFAULT_CERTAINTY), this.catalogApi.catalog()]);
      this.config.set(config);
      this.catalog.set(catalog);
    } catch (err) {
      this.error.set((err as Error).message);
    } finally {
      this.catalogLoaded.set(true);
    }
  }

  ngOnDestroy(): void {
    this.stopNoHandsWatch();
    this.pendingTimers.forEach((t) => clearInterval(t));
  }

  // ---------- CU-02 / CU-03 / CU-04 ----------

  protected async start(): Promise<void> {
    this.error.set(null);
    this.state.set('starting');
    if (!(await this.camera().start())) {
      this.state.set('idle');
      return;
    }
    try {
      const consultation = await this.api.start();
      this.consultation.set(consultation);
      this.records.set([]);
      this.live.set(null);
      this.repeat.set(false);
      this.exportMessage.set(null);
      this.state.set('active');
      this.startNoHandsWatch();
    } catch (err) {
      this.camera().stop();
      this.state.set('idle');
      this.error.set((err as Error).message);
    }
  }

  protected async pause(): Promise<void> {
    await this.run(async (c) => {
      const updated = await this.api.pause(c.id);
      this.camera().stop(); // RF-12: al pausar se detiene la captura
      this.detector.reset();
      this.noHands.set(false);
      this.preview.set(null);
      this.consultation.set(updated);
      this.state.set('paused');
    });
  }

  protected async resume(): Promise<void> {
    if (!(await this.camera().start())) return;
    await this.run(async (c) => {
      this.consultation.set(await this.api.resume(c.id));
      this.lastHandAt = performance.now();
      this.state.set('active');
    });
  }

  protected async finish(): Promise<void> {
    this.confirmingFinish.set(false);
    await this.run(async (c) => {
      const updated = await this.api.finish(c.id);
      this.camera().stop(); // RF-12: al finalizar se libera la cámara
      this.stopNoHandsWatch();
      this.pendingTimers.forEach((t) => clearInterval(t));
      this.pendingTimers.clear();
      this.countdown.set({});
      this.noHands.set(false);
      this.repeat.set(false);
      this.preview.set(null);
      this.consultation.set(updated);
      this.records.set(updated.records);
      this.state.set('finished');
    });
  }

  /** Empezar otra consulta deja el historial vacío (RF-09). */
  protected newConsultation(): void {
    this.consultation.set(null);
    this.records.set([]);
    this.live.set(null);
    this.exportMessage.set(null);
    this.state.set('idle');
  }

  // ---------- CU-05: reconocimiento ----------

  protected onFrame(frame: HandsFrame): void {
    if (this.state() !== 'active') return;
    if (frame.hands.length > 0) {
      this.lastHandAt = frame.timestamp;
      this.noHands.set(false); // RF-14: el aviso desaparece de inmediato
    }
    const completed = this.detector.push(frame.hands);
    if (completed) void this.handleSign(completed);
    if (++this.frameCount % 5 === 0) this.updatePreview(frame);
  }

  /** Indicador en vivo: la seña más parecida a la postura actual, aunque todavía no esté completada. */
  private updatePreview(frame: HandsFrame): void {
    const features = handFeatures(frame.hands);
    if (!features) {
      this.preview.set(null);
      return;
    }
    const match = bestMatch(features, this.catalog());
    this.preview.set({ word: match.sign?.word ?? null, score: match.score, level: levelFor(match.score, this.config()) });
  }

  private async handleSign(features: HandFeatures): Promise<void> {
    const consultation = this.consultation();
    if (!consultation) return;

    const recognizedAt = new Date();
    const match = bestMatch(features, this.catalog());
    const level = levelFor(match.score, this.config());
    const word = level === 'Low' ? null : (match.sign?.word ?? null);

    // Se muestra en cuanto se completa la seña, sin esperar al servidor (RNF-01)
    this.repeat.set(level === 'Low'); // CU-06
    this.live.set({ record: null, word, level, score: match.score });
    if (level === 'High' && word) this.speech.speak(word); // CU-08

    try {
      const record = await this.api.record(consultation.id, level === 'Low' ? null : (match.sign?.id ?? null), match.score, recognizedAt);
      this.upsert(record);
      if (this.live()?.score === match.score) this.live.set({ record, word: record.word, level: record.certaintyLevel, score: match.score });
      if (record.status === 'Pending') this.startConfirmationTimer(record); // CU-07
    } catch (err) {
      this.error.set(`No se guardó en el historial: ${(err as Error).message}`);
    }
  }

  // ---------- CU-07 ----------

  protected async confirm(record: TranslationRecord): Promise<void> {
    const updated = await this.review(record, 'confirm');
    if (updated?.word) this.speech.speak(updated.word); // RF-05: voz para amarillo confirmado
  }

  protected async discard(record: TranslationRecord): Promise<void> {
    if (await this.review(record, 'discard')) this.repeat.set(true); // RF-08: al descartar se pide repetir
  }

  private startConfirmationTimer(record: TranslationRecord): void {
    let left = this.config().confirmationSeconds;
    this.countdown.update((c) => ({ ...c, [record.id]: left }));
    const timer = setInterval(() => {
      left--;
      this.countdown.update((c) => ({ ...c, [record.id]: left }));
      if (left <= 0) void this.review(record, 'not-confirmed'); // se conserva como "no confirmada"
    }, 1000);
    this.pendingTimers.set(record.id, timer);
  }

  private async review(record: TranslationRecord, action: 'confirm' | 'discard' | 'not-confirmed'): Promise<TranslationRecord | null> {
    const consultation = this.consultation();
    if (!consultation) return null;
    this.stopTimer(record.id);
    try {
      const updated = await this.api.review(consultation.id, record.id, action);
      this.upsert(updated);
      const live = this.live();
      if (live?.record?.id === updated.id) this.live.set({ ...live, record: updated });
      return updated;
    } catch (err) {
      this.error.set((err as Error).message);
      return null;
    }
  }

  private stopTimer(id: string): void {
    const t = this.pendingTimers.get(id);
    if (t) clearInterval(t);
    this.pendingTimers.delete(id);
    this.countdown.update(({ [id]: _, ...rest }) => rest);
  }

  // ---------- CU-10 / CU-11 ----------

  protected async clearHistory(): Promise<void> {
    this.confirmingClear.set(false);
    await this.run(async (c) => {
      const updated = await this.api.clear(c.id);
      this.pendingTimers.forEach((t) => clearInterval(t));
      this.pendingTimers.clear();
      this.countdown.set({});
      this.records.set(updated.records);
      this.live.set(null);
    });
  }

  protected async download(): Promise<void> {
    await this.run(async (c) => {
      const text = await this.api.summary(c.id);
      const url = URL.createObjectURL(new Blob([text], { type: 'text/plain;charset=utf-8' }));
      const a = document.createElement('a');
      a.href = url;
      a.download = `signlink-consulta-${fileStamp(new Date(c.startedAt))}.txt`;
      a.click();
      URL.revokeObjectURL(url);
      this.exportMessage.set('Resumen descargado.');
    });
  }

  protected async copy(): Promise<void> {
    await this.run(async (c) => {
      await navigator.clipboard.writeText(await this.api.summary(c.id));
      this.exportMessage.set('Resumen copiado al portapapeles.');
    });
  }

  // ---------- utilidades ----------

  private upsert(record: TranslationRecord): void {
    this.records.update((list) => {
      const i = list.findIndex((r) => r.id === record.id);
      return i < 0 ? [...list, record] : list.map((r) => (r.id === record.id ? record : r));
    });
  }

  private async run(action: (c: Consultation) => Promise<void>): Promise<void> {
    const consultation = this.consultation();
    if (!consultation || this.busy()) return;
    this.busy.set(true);
    this.error.set(null);
    try {
      await action(consultation);
    } catch (err) {
      this.error.set((err as Error).message);
    } finally {
      this.busy.set(false);
    }
  }

  private startNoHandsWatch(): void {
    this.stopNoHandsWatch();
    this.lastHandAt = performance.now();
    this.noHandsTimer = setInterval(() => {
      if (this.state() === 'active' && performance.now() - this.lastHandAt > NO_HANDS_MS) this.noHands.set(true);
    }, 250);
  }

  private stopNoHandsWatch(): void {
    if (this.noHandsTimer) clearInterval(this.noHandsTimer);
    this.noHandsTimer = null;
  }
}

function fileStamp(d: Date): string {
  const p = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}${p(d.getMonth() + 1)}${p(d.getDate())}-${p(d.getHours())}${p(d.getMinutes())}`;
}
