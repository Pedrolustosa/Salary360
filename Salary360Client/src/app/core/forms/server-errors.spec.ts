import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup } from '@angular/forms';
import { applyServerErrors, serverErrorCode } from './server-errors';

describe('server-errors', () => {
  function form(): FormGroup {
    return new FormGroup({
      email: new FormControl(''),
      address: new FormGroup({ city: new FormControl('') }),
    });
  }

  function problem(errors: Record<string, string[]>): HttpErrorResponse {
    return new HttpErrorResponse({ status: 400, error: { code: 'validation_failed', errors } });
  }

  it('should mark matching controls with the first server code', () => {
    const group = form();

    const applied = applyServerErrors(group, problem({ email: ['required', 'invalid_email'], 'address.city': ['max_length'] }));

    expect(applied).toBe(true);
    expect(serverErrorCode(group.get('email'))).toBe('required');
    expect(serverErrorCode(group.get('address.city'))).toBe('max_length');
    expect(group.get('email')?.touched).toBe(true);
  });

  it('should ignore fields that do not exist in the form', () => {
    expect(applyServerErrors(form(), problem({ unknown: ['required'] }))).toBe(false);
  });

  it('should clear the server error when the value changes', () => {
    const group = form();
    applyServerErrors(group, problem({ email: ['required'] }));

    group.get('email')?.setValue('ana@example.com');

    expect(serverErrorCode(group.get('email'))).toBeNull();
  });

  it('should return null for controls without server errors', () => {
    expect(serverErrorCode(null)).toBeNull();
    expect(serverErrorCode(new FormControl(''))).toBeNull();
  });
});
