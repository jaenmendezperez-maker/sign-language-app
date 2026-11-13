import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { App } from './app';
import { AuthService } from './core/services/auth.service';

const links = (el: HTMLElement) => [...el.querySelectorAll('nav a')].map((a) => a.textContent?.trim());

describe('App', () => {
  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideHttpClient()],
    }).compileComponents();
  });

  it('shows only the vocabulary and login when nobody is logged in', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const el = fixture.nativeElement as HTMLElement;

    expect(links(el)).toEqual(['Vocabulario']);
    expect(el.querySelector('.user a')?.textContent).toContain('Iniciar sesión');
  });

  it('shows the doctor menu for medical staff', async () => {
    localStorage.setItem(
      'signlink.session',
      JSON.stringify({ token: 't', expiresAt: new Date(Date.now() + 60_000).toISOString(), name: 'Dra. Ana', role: 'MedicalStaff' }),
    );
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({ imports: [App], providers: [provideRouter([]), provideHttpClient()] }).compileComponents();

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    expect(links(fixture.nativeElement)).toEqual(['Traducir', 'Mis consultas', 'Vocabulario']);
    expect(TestBed.inject(AuthService).hasRole('MedicalStaff')).toBe(true);
  });
});
