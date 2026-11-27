import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { AuthResponse } from './interfaces/auth-response.interface';

const SESSION_KEY = 'salary360.session';

function session(overrides: Partial<AuthResponse> = {}): AuthResponse {
  return {
    userId: 'u1',
    email: 'ana@example.com',
    displayName: 'Ana',
    dateOfBirth: null,
    profilePictureUrl: null,
    accessToken: 'token',
    expiresAtUtc: new Date(Date.now() + 3_600_000).toISOString(),
    roles: ['Admin'],
    permissions: ['Profile.View'],
    ...overrides,
  };
}

describe('AuthService', () => {
  const router = { navigate: vi.fn() };
  let service: AuthService;
  let controller: HttpTestingController;

  function login(remember: boolean, response: AuthResponse = session()): void {
    service.login({ email: 'ana@example.com', password: 'Secret#123' }, remember).subscribe();
    controller.expectOne(`${environment.apiUrl}/auth/login`).flush(response);
  }

  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: Router, useValue: router }],
    });
    service = TestBed.inject(AuthService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should keep the session only in sessionStorage when remember is off', () => {
    login(false);

    expect(sessionStorage.getItem(SESSION_KEY)).not.toBeNull();
    expect(localStorage.getItem(SESSION_KEY)).toBeNull();
    expect(service.rememberedEmail()).toBe('');
    expect(service.isAuthenticated()).toBe(true);
  });

  it('should keep the session and the e-mail in localStorage when remember is on', () => {
    login(true);

    expect(localStorage.getItem(SESSION_KEY)).not.toBeNull();
    expect(service.rememberedEmail()).toBe('ana@example.com');
  });

  it('should never persist the password', () => {
    login(true);

    const stored = JSON.stringify({ ...localStorage }) + JSON.stringify({ ...sessionStorage });
    expect(stored).not.toContain('Secret#123');
  });

  it('should treat an expired session as signed out', () => {
    login(false, session({ expiresAtUtc: new Date(Date.now() - 1000).toISOString() }));

    expect(service.isAuthenticated()).toBe(false);
    expect(service.hasPermission('Profile.View')).toBe(false);
  });

  it('should answer role and permission checks from the session', () => {
    login(false);

    expect(service.hasRole('Admin')).toBe(true);
    expect(service.hasRole('User')).toBe(false);
    expect(service.hasPermission('Profile.View')).toBe(true);
    expect(service.hasPermission('Users.View')).toBe(false);
  });

  it('should clear the session and go to the login on logout', () => {
    login(true);

    service.logout();

    expect(service.session()).toBeNull();
    expect(localStorage.getItem(SESSION_KEY)).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
