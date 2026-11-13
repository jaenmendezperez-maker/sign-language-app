import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CertaintyConfig, Consultation, ConsultationSummary, TranslationRecord } from '../../models/consultation.model';

/** Llamadas de las consultas del médico. Al servidor solo llegan palabras y puntajes (RNF-06). */
@Injectable({ providedIn: 'root' })
export class ConsultationApiService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api`;
  private readonly base = `${this.api}/consultations`;

  certainty(): Promise<CertaintyConfig> {
    return firstValueFrom(this.http.get<CertaintyConfig>(`${this.api}/config/certainty`));
  }

  start(): Promise<Consultation> {
    return firstValueFrom(this.http.post<Consultation>(this.base, null));
  }

  list(take = 50): Promise<ConsultationSummary[]> {
    return firstValueFrom(this.http.get<ConsultationSummary[]>(this.base, { params: { take } }));
  }

  get(id: string): Promise<Consultation> {
    return firstValueFrom(this.http.get<Consultation>(`${this.base}/${id}`));
  }

  pause(id: string): Promise<Consultation> {
    return firstValueFrom(this.http.post<Consultation>(`${this.base}/${id}/pause`, null));
  }

  resume(id: string): Promise<Consultation> {
    return firstValueFrom(this.http.post<Consultation>(`${this.base}/${id}/resume`, null));
  }

  finish(id: string): Promise<Consultation> {
    return firstValueFrom(this.http.post<Consultation>(`${this.base}/${id}/finish`, null));
  }

  record(id: string, signId: string | null, similarityScore: number, recognizedAt: Date): Promise<TranslationRecord> {
    return firstValueFrom(
      this.http.post<TranslationRecord>(`${this.base}/${id}/records`, { signId, similarityScore, recognizedAt: recognizedAt.toISOString() }),
    );
  }

  review(id: string, recordId: string, action: 'confirm' | 'discard' | 'not-confirmed'): Promise<TranslationRecord> {
    return firstValueFrom(this.http.post<TranslationRecord>(`${this.base}/${id}/records/${recordId}/${action}`, null));
  }

  clear(id: string): Promise<Consultation> {
    return firstValueFrom(this.http.delete<Consultation>(`${this.base}/${id}/records`));
  }

  /** Resumen en texto plano (RF-13). */
  summary(id: string): Promise<string> {
    return firstValueFrom(this.http.get(`${this.base}/${id}/summary`, { responseType: 'text' })).then((t) => t.replace(/^﻿/, ''));
  }
}
