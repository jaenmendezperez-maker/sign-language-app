import { Routes } from '@angular/router';

import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [roleGuard('MedicalStaff')],
    loadComponent: () => import('./features/translate/translate').then((m) => m.Translate),
    title: 'Traducir · SignLink',
  },
  {
    path: 'consultas',
    canActivate: [roleGuard('MedicalStaff')],
    loadComponent: () => import('./features/consultations/consultations').then((m) => m.Consultations),
    title: 'Mis consultas · SignLink',
  },
  {
    path: 'admin/registrar-sena',
    canActivate: [roleGuard('Administrator')],
    loadComponent: () => import('./features/admin/register-sign').then((m) => m.RegisterSign),
    title: 'Registrar seña · SignLink',
  },
  {
    path: 'vocabulario',
    loadComponent: () => import('./features/vocabulary/vocabulary').then((m) => m.Vocabulary),
    title: 'Vocabulario · SignLink',
  },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login').then((m) => m.Login),
    title: 'Iniciar sesión · SignLink',
  },
  { path: '**', redirectTo: '' },
];
