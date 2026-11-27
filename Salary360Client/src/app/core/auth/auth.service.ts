import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { withoutErrorToast } from '../api/api-error';
import { UserStatus } from './enums/user-status.enum';
import { AuthResponse } from './interfaces/auth-response.interface';
import { LoginRequest } from './interfaces/login-request.interface';
import { RegisterRequest } from './interfaces/register-request.interface';
import { RoleSummary } from './interfaces/role-summary.interface';
import { UpdateProfileRequest } from './interfaces/update-profile-request.interface';
import { UserProfile } from './interfaces/user-profile.interface';

const SESSION_KEY = 'salary360.session';
const REMEMBERED_EMAIL_KEY = 'salary360.remembered-email';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly sessionState = signal<AuthResponse | null>(this.restore());

  readonly session = this.sessionState.asReadonly();
  readonly isAuthenticated = computed(() => {
    const session = this.sessionState();
    return session !== null && new Date(session.expiresAtUtc).getTime() > Date.now();
  });

  login(request: LoginRequest, remember: boolean): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, request)
      .pipe(tap((session) => this.store(session, remember, request.email)));
  }

  register(request: RegisterRequest): Observable<UserProfile> {
    return this.http.post<UserProfile>(`${this.baseUrl}/register`, request);
  }

  confirmEmail(userId: string, token: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/confirm-email`, { userId, token }, { context: withoutErrorToast() });
  }

  resendConfirmation(email: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/resend-confirmation`, { email });
  }

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/me`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.baseUrl}/me`, request).pipe(tap((profile) => this.patchSession(profile)));
  }

  listUsers(): Observable<UserProfile[]> {
    return this.http.get<UserProfile[]>(`${environment.apiUrl}/users`);
  }

  listRoles(): Observable<RoleSummary[]> {
    return this.http.get<RoleSummary[]>(`${environment.apiUrl}/roles`);
  }

  setUserStatus(userId: string, status: UserStatus): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${environment.apiUrl}/users/${userId}/status`, { status });
  }

  updateRolePermissions(roleName: string, permissions: string[]): Observable<RoleSummary> {
    return this.http.put<RoleSummary>(`${environment.apiUrl}/roles/${encodeURIComponent(roleName)}/permissions`, {
      permissions,
    });
  }

  hasRole(role: string): boolean {
    return this.isAuthenticated() && (this.sessionState()?.roles ?? []).includes(role);
  }

  hasPermission(permission: string): boolean {
    return this.isAuthenticated() && (this.sessionState()?.permissions ?? []).includes(permission);
  }

  rememberedEmail(): string {
    try {
      return localStorage.getItem(REMEMBERED_EMAIL_KEY) ?? '';
    } catch {
      return '';
    }
  }

  logout(): void {
    this.sessionState.set(null);
    try {
      sessionStorage.removeItem(SESSION_KEY);
      localStorage.removeItem(SESSION_KEY);
    } catch {
      return;
    } finally {
      void this.router.navigate(['/login']);
    }
  }

  private patchSession(profile: UserProfile): void {
    const current = this.sessionState();
    if (!current) return;

    const next = { ...current, displayName: profile.displayName,
      dateOfBirth: profile.dateOfBirth,
      profilePictureUrl: profile.profilePictureUrl,
    };
    this.sessionState.set(next);
    try {
      const storage = localStorage.getItem(SESSION_KEY) ? localStorage : sessionStorage;
      storage.setItem(SESSION_KEY, JSON.stringify(next));
    } catch {
      return;
    }
  }

  private store(session: AuthResponse, remember: boolean, email: string): void {
    this.sessionState.set(session);
    try {
      sessionStorage.removeItem(SESSION_KEY);
      localStorage.removeItem(SESSION_KEY);
      if (remember) {
        localStorage.setItem(SESSION_KEY, JSON.stringify(session));
        localStorage.setItem(REMEMBERED_EMAIL_KEY, email);
      } else {
        sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));
        localStorage.removeItem(REMEMBERED_EMAIL_KEY);
      }
    } catch {
      return;
    }
  }

  private restore(): AuthResponse | null {
    try {
      const raw = localStorage.getItem(SESSION_KEY) ?? sessionStorage.getItem(SESSION_KEY);
      return raw ? (JSON.parse(raw) as AuthResponse) : null;
    } catch {
      return null;
    }
  }
}
