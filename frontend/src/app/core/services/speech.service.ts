import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'signlink.voice';

/** Texto a voz con la Web Speech API del navegador, en español (RF-05). */
@Injectable({ providedIn: 'root' })
export class SpeechService {
  readonly supported = typeof window !== 'undefined' && 'speechSynthesis' in window;
  readonly enabled = signal(this.restore());

  toggle(): void {
    this.enabled.update((v) => !v);
    try {
      localStorage.setItem(STORAGE_KEY, String(this.enabled()));
    } catch {
      /* preferencia solo para esta pestaña */
    }
    if (!this.enabled() && this.supported) speechSynthesis.cancel();
  }

  /** No bloquea: el texto ya está en pantalla cuando empieza la voz (RF-05). */
  speak(text: string): void {
    if (!this.supported || !this.enabled() || !text) return;
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = 'es-MX';
    speechSynthesis.cancel();
    speechSynthesis.speak(utterance);
  }

  private restore(): boolean {
    try {
      return localStorage.getItem(STORAGE_KEY) === 'true';
    } catch {
      return false;
    }
  }
}
