import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginResult, UserRole } from '../../models/auth.model';

const STORAGE_KEY = 'signlink.session';

/** Sesión del usuario (CU-01). El paciente no inicia sesión. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly session = signal<LoginResult | null>(this.restore());

  readonly user = this.session.asReadonly();
  readonly isAuthenticated = computed(() => this.session() !== null);
  readonly role = computed(() => this.session()?.role ?? null);

  get token(): string | null {
    const s = this.session();
    if (!s) return null;
    if (new Date(s.expiresAt).getTime() <= Date.now()) {
      this.clear();
      return null;
    }
    return s.token;
  }

  hasRole(role: UserRole): boolean {
    return this.token !== null && this.session()?.role === role;
  }

  async login(email: string, password: string): Promise<LoginResult> {
    const result = await firstValueFrom(this.http.post<LoginResult>(`${environment.apiUrl}/api/auth/login`, { email, password }));
    this.session.set(result);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(result));
    } catch {
      /* almacenamiento no disponible: la sesión dura lo que la pestaña */
    }
    return result;
  }

  /** Página de inicio según el rol. */
  homeFor(role: UserRole | null): string {
    return role === 'Administrator' ? '/admin/registrar-sena' : role === 'MedicalStaff' ? '/' : '/vocabulario';
  }

  logout(): void {
    this.clear();
    void this.router.navigate(['/login']);
  }

  private clear(): void {
    this.session.set(null);
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      /* nada que limpiar */
    }
  }

  private restore(): LoginResult | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      const s = raw ? (JSON.parse(raw) as LoginResult) : null;
      return s && new Date(s.expiresAt).getTime() > Date.now() ? s : null;
    } catch {
      return null;
    }
  }
}
