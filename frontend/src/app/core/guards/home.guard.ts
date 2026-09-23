import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

import { AuthService } from '../services/auth.service';

export const homeGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const currentUser = authService.currentUser();

  if (currentUser) {
    return currentUser.role === 'Interviewer'
      ? router.createUrlTree(['/dashboard'])
      : router.createUrlTree(['/my-dashboard']);
  }

  return authService.loadCurrentUser().pipe(
    map(user => {
      return user.role === 'Interviewer'
        ? router.createUrlTree(['/dashboard'])
        : router.createUrlTree(['/my-dashboard']);
    }),
    catchError(() => {
      authService.login();
      return of(false);
    })
  );
};
