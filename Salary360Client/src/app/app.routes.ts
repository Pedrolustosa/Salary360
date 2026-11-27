import { Routes } from '@angular/router';
import { Permission } from './core/auth/enums/permission.enum';
import { authGuard, guestGuard, permissionGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login').then((m) => m.Login),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register').then((m) => m.Register),
  },
  {
    path: 'check-email',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/check-email').then((m) => m.CheckEmail),
  },
  {
    path: 'confirm-email',
    loadComponent: () => import('./features/auth/confirm-email').then((m) => m.ConfirmEmail),
  },
  {
    path: 'resend-confirmation',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/resend-confirmation').then((m) => m.ResendConfirmation),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell').then((m) => m.Shell),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () => import('./features/home/home').then((m) => m.Home),
      },
      {
        path: 'profile',
        canActivate: [permissionGuard(Permission.ProfileView)],
        loadComponent: () => import('./features/profile/profile-page').then((m) => m.ProfilePage),
      },
      {
        path: 'admin',
        canActivate: [permissionGuard(Permission.RolesView)],
        loadComponent: () => import('./features/admin/admin-page').then((m) => m.AdminPage),
      },
    ],
  },
  {
    path: 'error/:code',
    loadComponent: () => import('./features/error/error-page').then((m) => m.ErrorPage),
  },
  { path: '**', redirectTo: 'error/404' },
];
