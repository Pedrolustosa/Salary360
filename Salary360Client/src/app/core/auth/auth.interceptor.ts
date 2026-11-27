import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(AuthService).session();

  if (session && request.url.startsWith(environment.apiUrl)) {
    request = request.clone({ setHeaders: { Authorization: `Bearer ${session.accessToken}` } });
  }

  return next(request);
};
