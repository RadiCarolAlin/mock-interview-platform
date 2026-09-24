import { AbstractControl, ValidatorFn } from '@angular/forms';

export const nonBlank: ValidatorFn = control =>
  typeof control.value === 'string' && !control.value.trim() ? { required: true } : null;

export const integer: ValidatorFn = control =>
  Number.isInteger(control.value) ? null : { integer: true };

export function fieldError(control: AbstractControl): string {
  if (!control.touched || !control.errors) return '';
  const errors = control.errors;
  if (errors['required']) return 'This field is required.';
  if (errors['email']) return 'Enter a valid email address.';
  if (errors['maxlength']) return 'Use at most ' + errors['maxlength'].requiredLength + ' characters.';
  if (errors['min']) return 'The minimum value is ' + errors['min'].min + '.';
  if (errors['max']) return 'The maximum value is ' + errors['max'].max + '.';
  if (errors['integer']) return 'Enter a whole number.';
  return 'Select or enter a valid value.';
}
