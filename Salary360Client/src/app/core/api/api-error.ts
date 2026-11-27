import { HttpContext, HttpContextToken, HttpErrorResponse } from '@angular/common/http';
import { ApiProblem } from './interfaces/api-problem.interface';

export const SKIP_ERROR_TOAST = new HttpContextToken<boolean>(() => false);

export function withoutErrorToast(): HttpContext {
  return new HttpContext().set(SKIP_ERROR_TOAST, true);
}

export function apiProblem(error: unknown): ApiProblem | null {
  if (error instanceof HttpErrorResponse && error.error !== null && typeof error.error === 'object') {
    return error.error as ApiProblem;
  }
  return null;
}

export function apiErrorCodes(error: unknown): string[] {
  const codes = apiProblem(error)?.codes;
  return Array.isArray(codes) ? codes.filter((code): code is string => typeof code === 'string') : [];
}

export function apiErrorCode(error: unknown): string | undefined {
  const code = apiProblem(error)?.code;
  return typeof code === 'string' ? code : undefined;
}

export function apiFieldErrors(error: unknown): Record<string, string[]> {
  const errors = apiProblem(error)?.errors;
  if (errors === null || typeof errors !== 'object' || errors === undefined) return {};
  return Object.fromEntries(
    Object.entries(errors).filter(([field, codes]) => field !== 'identity' && Array.isArray(codes)),
  );
}

export function apiRetryAfterSeconds(error: unknown): number | null {
  const fromBody = apiProblem(error)?.retryAfterSeconds;
  if (typeof fromBody === 'number' && fromBody > 0) return fromBody;

  if (error instanceof HttpErrorResponse) {
    const header = Number(error.headers?.get('Retry-After'));
    if (Number.isFinite(header) && header > 0) return header;
  }
  return null;
}

export function formatRetryAfter(totalSeconds: number): string {
  const seconds = Math.max(0, Math.ceil(totalSeconds));
  if (seconds < 60) return `${seconds} s`;
  const minutes = Math.floor(seconds / 60);
  const rest = seconds % 60;
  return rest === 0 ? `${minutes} min` : `${minutes} min ${rest} s`;
}
