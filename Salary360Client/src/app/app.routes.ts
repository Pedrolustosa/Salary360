import { Routes } from '@angular/router';
import { Permission } from './core/auth/enums/permission.enum';
import { authGuard, guestGuard, permissionGuard } from './core/auth/auth.guard';
import { canDeactivateGuard } from './core/forms/can-deactivate.guard';

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
      {
        path: 'salaries',
        canActivate: [permissionGuard(Permission.SalariesView)],
        loadComponent: () => import('./features/salaries/salary-list/salary-list').then((m) => m.SalaryList),
      },
      {
        path: 'salaries/new',
        canActivate: [permissionGuard(Permission.SalariesCreate)],
        canDeactivate: [canDeactivateGuard],
        loadComponent: () => import('./features/salaries/salary-form/salary-form').then((m) => m.SalaryForm),
      },
      {
        path: 'salaries/:id',
        canActivate: [permissionGuard(Permission.SalariesUpdate)],
        canDeactivate: [canDeactivateGuard],
        loadComponent: () => import('./features/salaries/salary-form/salary-form').then((m) => m.SalaryForm),
      },
      {
        path: 'evolution',
        canActivate: [permissionGuard(Permission.SalariesView)],
        loadComponent: () => import('./features/evolution/evolution-page').then((m) => m.EvolutionPage),
      },
      {
        path: 'salary-intelligence',
        canActivate: [permissionGuard(Permission.SalariesView)],
        loadComponent: () =>
          import('./features/salary-intelligence/salary-intelligence/salary-intelligence').then(
            (m) => m.SalaryIntelligence,
          ),
      },
    ],
  },
  {
    path: 'error/:code',
    loadComponent: () => import('./features/error/error-page').then((m) => m.ErrorPage),
  },
  { path: '**', redirectTo: 'error/404' },
];
