import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  return next(request).pipe(catchError((error: unknown) => {
    if (request.url.startsWith('/api/') && request.url !== '/api/auth/me' && error instanceof HttpErrorResponse && error.status === 401) {
      auth.handleAccessError(error);
    }
    return throwError(() => error);
  }));
};
