import { HttpErrorResponse } from '@angular/common/http';
import { apiErrorMessage } from './api-error';

describe('API error messages', () => {
  it('shows the safe business conflict detail from ProblemDetails', () => {
    expect(apiErrorMessage(new HttpErrorResponse({ status: 409, error: {
      status: 409, detail: 'Feedback already exists for this interview.'
    } }))).toBe('Feedback already exists for this interview.');
  });

  it('hides unexpected server details and distinguishes network failures', () => {
    expect(apiErrorMessage(new HttpErrorResponse({ status: 500, error: {
      detail: 'Internal SQL exception with secret connection details'
    } }))).toBe('Something went wrong. Please try again.');
    expect(apiErrorMessage(new HttpErrorResponse({ status: 0 })))
      .toBe('Unable to reach the server. Check your connection and try again.');
  });
});
