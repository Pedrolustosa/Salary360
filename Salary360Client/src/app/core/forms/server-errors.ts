import { AbstractControl, FormGroup } from '@angular/forms';
import { apiFieldErrors } from '../api/api-error';

export const SERVER_ERROR_KEY = 'serverError';

export function applyServerErrors(form: FormGroup, error: unknown): boolean {
  let applied = false;

  for (const [field, codes] of Object.entries(apiFieldErrors(error))) {
    const control = form.get(field);
    if (!control || codes.length === 0) continue;

    control.setErrors({ ...(control.errors ?? {}), [SERVER_ERROR_KEY]: codes[0] });
    control.markAsTouched();
    applied = true;
  }

  return applied;
}

export function serverErrorCode(control: AbstractControl | null): string | null {
  const code = control?.errors?.[SERVER_ERROR_KEY] as unknown;
  return typeof code === 'string' ? code : null;
}
