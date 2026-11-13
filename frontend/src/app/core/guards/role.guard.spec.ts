import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';

import { roleGuard } from './role.guard';

const session = (role: string) =>
  localStorage.setItem('signlink.session', JSON.stringify({ token: 't', expiresAt: new Date(Date.now() + 60_000).toISOString(), name: 'X', role }));

const run = (role: 'Administrator' | 'MedicalStaff', url = '/admin/registrar-sena') =>
  TestBed.runInInjectionContext(() =>
    roleGuard(role)({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
  ) as boolean | UrlTree;

describe('roleGuard (RF-11: solo el administrador registra señas)', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([]), provideHttpClient()] });
  });

  it('sends anonymous users to the login with the return url', () => {
    const result = run('Administrator');
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe('/login?returnUrl=%2Fadmin%2Fregistrar-sena');
  });

  it('sends a doctor away from the admin page', () => {
    session('MedicalStaff');
    const result = run('Administrator');
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe('/');
  });

  it('lets the administrator in', () => {
    session('Administrator');
    expect(run('Administrator')).toBe(true);
  });
});
