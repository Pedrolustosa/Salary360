import { EnvironmentInjector } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { AuthService } from './auth.service';
import { authGuard, guestGuard, permissionGuard, roleGuard } from './auth.guard';

describe('guards', () => {
  const auth = { isAuthenticated: vi.fn(), hasRole: vi.fn(), hasPermission: vi.fn() };
  const route = {} as ActivatedRouteSnapshot;
  const state = {} as RouterStateSnapshot;

  function run(guard: (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => unknown): unknown {
    return TestBed.inject(EnvironmentInjector).runInContext(() => guard(route, state));
  }

  function path(result: unknown): string {
    return TestBed.inject(Router).serializeUrl(result as UrlTree);
  }

  beforeEach(() => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({ providers: [provideRouter([]), { provide: AuthService, useValue: auth }] });
  });

  it('should send anonymous users to the login', () => {
    auth.isAuthenticated.mockReturnValue(false);

    expect(path(run(authGuard))).toBe('/login');
  });

  it('should let authenticated users through', () => {
    auth.isAuthenticated.mockReturnValue(true);

    expect(run(authGuard)).toBe(true);
  });

  it('should send authenticated users away from guest pages', () => {
    auth.isAuthenticated.mockReturnValue(true);

    expect(path(run(guestGuard))).toBe('/');
  });

  it('should send users without the role or permission to the 403 page', () => {
    auth.hasRole.mockReturnValue(false);
    auth.hasPermission.mockReturnValue(false);

    expect(path(run(roleGuard('Admin')))).toBe('/error/403');
    expect(path(run(permissionGuard('Roles.View')))).toBe('/error/403');
  });

  it('should allow users with the role and permission', () => {
    auth.hasRole.mockReturnValue(true);
    auth.hasPermission.mockReturnValue(true);

    expect(run(roleGuard('Admin'))).toBe(true);
    expect(run(permissionGuard('Roles.View'))).toBe(true);
  });
});
