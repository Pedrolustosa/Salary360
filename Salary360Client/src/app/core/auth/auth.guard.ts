import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

const FORBIDDEN_ROUTE = ['/error', '403'];

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.isAuthenticated() ? true : inject(Router).createUrlTree(['/login']);
};

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.isAuthenticated() ? inject(Router).createUrlTree(['/']) : true;
};

export const roleGuard =
  (role: string): CanActivateFn =>
  () => (inject(AuthService).hasRole(role) ? true : inject(Router).createUrlTree(FORBIDDEN_ROUTE));

export const permissionGuard =
  (permission: string): CanActivateFn =>
  () => (inject(AuthService).hasPermission(permission) ? true : inject(Router).createUrlTree(FORBIDDEN_ROUTE));
