import { HttpErrorResponse } from '@angular/common/http';

export function apiErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Something went wrong. Please try again.';
  switch (error.status) {
    case 0: return 'Unable to reach the server. Check your connection and try again.';
    case 401: return 'Your session has expired. Please sign in again.';
    case 403: return 'You do not have permission to access this resource.';
    case 404: return 'The requested resource was not found.';
    case 400: {
      const errors = error.error?.errors;
      const messages = errors && typeof errors === 'object'
        ? Object.values(errors).flat().filter((value): value is string => typeof value === 'string') : [];
      return messages.length ? messages.join(' ') : 'Please check the values you entered.';
    }
    case 409: return error.error?.status === 409 && typeof error.error?.detail === 'string'
      ? error.error.detail : 'This action conflicts with the current data. Please refresh and try again.';
    default: return 'Something went wrong. Please try again.';
  }
}
