import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { UserRole } from '../../models/auth.model';
import { AuthService } from '../services/auth.service';

/** Permite la ruta solo al rol indicado; si no, manda al login (o al inicio de su rol). */
export const roleGuard =
  (role: UserRole): CanActivateFn =>
  (_route, state) => {
    const auth = inject(AuthService);
    const router = inject(Router);
    if (auth.hasRole(role)) return true;
    if (auth.token) return router.parseUrl(auth.homeFor(auth.role()));
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  };
