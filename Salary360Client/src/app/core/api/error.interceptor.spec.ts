import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { withoutErrorToast } from './api-error';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  const auth = { isAuthenticated: vi.fn(), logout: vi.fn() };
  const router = { navigate: vi.fn() };
  const toastr = { error: vi.fn(), warning: vi.fn(), info: vi.fn() };
  const known = new Set([
    'errors.network', 'errors.forbidden', 'errors.notFound', 'errors.conflict', 'errors.invalidCredentials',
    'errors.userInactive', 'errors.accountLocked', 'errors.tooManyRequests', 'errors.checkFields',
    'errors.sessionExpired', 'errors.serverError', 'errors.identity.DuplicateEmail',
  ]);
  const translate = {
    instant: vi.fn((key: string) => (known.has(key) ? `tr:${key}` : key)),
  };

  let http: HttpClient;
  let controller: HttpTestingController;

  function fail(method: string, path: string, status: number, body: object, headers: Record<string, string> = {}): void {
    http.request(method, `${environment.apiUrl}${path}`).subscribe({ error: () => undefined });
    controller.expectOne(`${environment.apiUrl}${path}`).flush(body, { status, statusText: 'x', headers });
  }

  beforeEach(() => {
    vi.clearAllMocks();
    auth.isAuthenticated.mockReturnValue(true);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
        { provide: Router, useValue: router },
        { provide: ToastrService, useValue: toastr },
        { provide: TranslateService, useValue: translate },
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should show a single warning toast for a 400 with field errors', () => {
    fail('POST', '/auth/register', 400, { code: 'validation_failed', errors: { email: ['required'], password: ['min_length'] } });

    expect(toastr.warning).toHaveBeenCalledTimes(1);
    expect(toastr.warning).toHaveBeenCalledWith('tr:errors.checkFields', 'toast.warningTitle');
    expect(toastr.error).not.toHaveBeenCalled();
  });

  it('should show an error toast for invalid credentials without logging out', () => {
    fail('POST', '/auth/login', 401, { code: 'invalid_credentials' });

    expect(toastr.error).toHaveBeenCalledWith('tr:errors.invalidCredentials', 'toast.errorTitle');
    expect(auth.logout).not.toHaveBeenCalled();
  });

  it('should log out and inform the user when an authenticated call returns 401', () => {
    fail('GET', '/users', 401, { code: 'unauthorized' });

    expect(auth.logout).toHaveBeenCalledTimes(1);
    expect(toastr.info).toHaveBeenCalledWith('tr:errors.sessionExpired', 'toast.infoTitle');
  });

  it('should not toast when the e-mail is not confirmed', () => {
    fail('POST', '/auth/login', 403, { code: 'email_not_confirmed' });

    expect(toastr.error).not.toHaveBeenCalled();
    expect(toastr.warning).not.toHaveBeenCalled();
  });

  it('should show an error toast when the account is inactive', () => {
    fail('POST', '/auth/login', 403, { code: 'user_inactive' });

    expect(toastr.error).toHaveBeenCalledWith('tr:errors.userInactive', 'toast.errorTitle');
  });

  it('should tell how long a locked account has to wait', () => {
    fail('POST', '/auth/login', 403, { code: 'account_locked', retryAfterSeconds: 270 });

    expect(translate.instant).toHaveBeenCalledWith('errors.accountLocked', { time: '4 min 30 s' });
    expect(toastr.error).toHaveBeenCalledTimes(1);
  });

  it('should redirect a forbidden navigation to the 403 page', () => {
    fail('GET', '/users', 403, { code: 'forbidden' });

    expect(router.navigate).toHaveBeenCalledWith(['/error', '403']);
    expect(toastr.warning).not.toHaveBeenCalled();
  });

  it('should warn instead of redirecting when a forbidden action is not a navigation', () => {
    fail('PUT', '/users/1/status', 403, { code: 'forbidden' });

    expect(router.navigate).not.toHaveBeenCalled();
    expect(toastr.warning).toHaveBeenCalledWith('tr:errors.forbidden', 'toast.warningTitle');
  });

  it('should redirect a missing resource to the 404 page on navigation', () => {
    fail('GET', '/roles', 404, { code: 'not_found' });

    expect(router.navigate).toHaveBeenCalledWith(['/error', '404']);
  });

  it('should warn about duplicates with the translated identity code on 409', () => {
    fail('POST', '/auth/register', 409, { code: 'conflict', codes: ['DuplicateEmail'] });

    expect(toastr.warning).toHaveBeenCalledWith('tr:errors.identity.DuplicateEmail', 'toast.warningTitle');
  });

  it('should warn with the remaining time on 429 using the Retry-After header', () => {
    fail('POST', '/auth/login', 429, { code: 'too_many_requests' }, { 'Retry-After': '90' });

    expect(translate.instant).toHaveBeenCalledWith('errors.tooManyRequests', { time: '1 min 30 s' });
    expect(toastr.warning).toHaveBeenCalledTimes(1);
  });

  it('should show a generic toast and open the 500 page on navigation', () => {
    fail('GET', '/users', 500, { code: 'internal_error', traceId: 'abc' });

    expect(toastr.error).toHaveBeenCalledWith('tr:errors.serverError', 'toast.errorTitle');
    expect(router.navigate).toHaveBeenCalledWith(['/error', '500']);
  });

  it('should not open the 500 page for a failed action', () => {
    fail('PUT', '/auth/me', 500, { code: 'internal_error' });

    expect(toastr.error).toHaveBeenCalledTimes(1);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('should show a connection error when there is no response', () => {
    http.get(`${environment.apiUrl}/users`).subscribe({ error: () => undefined });
    controller.expectOne(`${environment.apiUrl}/users`).error(new ProgressEvent('error'));

    expect(toastr.error).toHaveBeenCalledWith('tr:errors.network', 'toast.errorTitle');
  });

  it('should leave the error to the component when the toast is skipped', () => {
    http.get(`${environment.apiUrl}/auth/me`, { context: withoutErrorToast() }).subscribe({ error: () => undefined });
    controller.expectOne(`${environment.apiUrl}/auth/me`).flush({ code: 'internal_error' }, { status: 500, statusText: 'x' });

    expect(toastr.error).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('should ignore requests to other origins', () => {
    http.get('https://example.com/data').subscribe({ error: () => undefined });
    controller.expectOne('https://example.com/data').flush({}, { status: 500, statusText: 'x' });

    expect(toastr.error).not.toHaveBeenCalled();
  });
});
