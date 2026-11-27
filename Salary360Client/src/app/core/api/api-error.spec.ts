import { HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { apiErrorCode, apiErrorCodes, apiFieldErrors, apiProblem, apiRetryAfterSeconds, formatRetryAfter } from './api-error';

function response(status: number, error: unknown, headers?: Record<string, string>): HttpErrorResponse {
  return new HttpErrorResponse({ status, error, headers: new HttpHeaders(headers) });
}

describe('api-error', () => {
  it('should read the code and identity codes from a problem body', () => {
    const error = response(400, { code: 'identity_failed', codes: ['DuplicateEmail', 3] });

    expect(apiErrorCode(error)).toBe('identity_failed');
    expect(apiErrorCodes(error)).toEqual(['DuplicateEmail']);
  });

  it('should return empty values when the body is not a problem', () => {
    const error = response(500, 'plain text');

    expect(apiProblem(error)).toBeNull();
    expect(apiErrorCode(error)).toBeUndefined();
    expect(apiErrorCodes(error)).toEqual([]);
    expect(apiFieldErrors(error)).toEqual({});
  });

  it('should ignore values that are not HttpErrorResponse', () => {
    expect(apiErrorCode(new Error('x'))).toBeUndefined();
    expect(apiRetryAfterSeconds(null)).toBeNull();
  });

  it('should expose field errors except the identity bucket', () => {
    const error = response(400, {
      code: 'validation_failed',
      errors: { email: ['required'], 'address.city': ['max_length'], identity: ['DuplicateEmail'] },
    });

    expect(apiFieldErrors(error)).toEqual({ email: ['required'], 'address.city': ['max_length'] });
  });

  it('should prefer retryAfterSeconds from the body and fall back to the header', () => {
    expect(apiRetryAfterSeconds(response(429, { retryAfterSeconds: 42 }, { 'Retry-After': '10' }))).toBe(42);
    expect(apiRetryAfterSeconds(response(429, {}, { 'Retry-After': '10' }))).toBe(10);
    expect(apiRetryAfterSeconds(response(429, {}))).toBeNull();
  });

  it.each([
    [0, '0 s'],
    [45, '45 s'],
    [60, '1 min'],
    [270, '4 min 30 s'],
    [12.2, '13 s'],
  ])('should format %s seconds as %s', (seconds, expected) => {
    expect(formatRetryAfter(seconds)).toBe(expected);
  });
});
