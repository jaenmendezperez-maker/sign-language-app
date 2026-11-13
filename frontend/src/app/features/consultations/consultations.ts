import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';

import { ConsultationApiService } from '../../core/services/consultation-api.service';
import { Consultation, ConsultationSummary } from '../../models/consultation.model';
import { CertaintyBadge } from '../../shared/components/certainty-badge';
import { ScorePipe } from '../../shared/pipes/score.pipe';

const STATUS: Record<ConsultationSummary['status'], string> = { Active: 'Activa', Paused: 'En pausa', Finished: 'Finalizada' };

/** CU-10 / CU-11: consultas anteriores del médico, con su historial y exportación. */
@Component({
  selector: 'app-consultations',
  imports: [DatePipe, CertaintyBadge, ScorePipe],
  templateUrl: './consultations.html',
})
export class Consultations implements OnInit {
  private readonly api = inject(ConsultationApiService);

  protected readonly statusLabel = STATUS;
  protected readonly items = signal<ConsultationSummary[]>([]);
  protected readonly selected = signal<Consultation | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly message = signal<string | null>(null);

  async ngOnInit(): Promise<void> {
    try {
      this.items.set(await this.api.list());
    } catch (err) {
      this.error.set((err as Error).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected async open(id: string): Promise<void> {
    this.message.set(null);
    try {
      this.selected.set(this.selected()?.id === id ? null : await this.api.get(id));
    } catch (err) {
      this.error.set((err as Error).message);
    }
  }

  protected async download(c: ConsultationSummary): Promise<void> {
    try {
      const text = await this.api.summary(c.id);
      const url = URL.createObjectURL(new Blob([text], { type: 'text/plain;charset=utf-8' }));
      const a = document.createElement('a');
      a.href = url;
      a.download = `signlink-consulta-${c.date}.txt`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      this.error.set((err as Error).message);
    }
  }

  protected async copy(c: ConsultationSummary): Promise<void> {
    try {
      await navigator.clipboard.writeText(await this.api.summary(c.id));
      this.message.set('Resumen copiado al portapapeles.');
    } catch (err) {
      this.error.set((err as Error).message);
    }
  }
}
