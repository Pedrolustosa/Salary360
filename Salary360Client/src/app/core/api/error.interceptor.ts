import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Injector, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import {
  SKIP_ERROR_TOAST,
  apiErrorCode,
  apiErrorCodes,
  apiFieldErrors,
  apiRetryAfterSeconds,
  formatRetryAfter,
} from './api-error';
import { ToastLevel } from './enums/toast-level.enum';

const CREDENTIALS_CALL = /\/auth\/(login|register|confirm-email|resend-confirmation)/;

const MESSAGE_KEY_BY_STATUS: Record<number, string> = {
  0: 'errors.network',
  400: 'errors.validation',
  401: 'errors.unauthorized',
  403: 'errors.forbidden',
  404: 'errors.notFound',
  409: 'errors.conflict',
  422: 'errors.businessRule',
  429: 'errors.tooManyRequests',
};

const MESSAGE_KEY_BY_CODE: Record<string, string> = {
  invalid_credentials: 'errors.invalidCredentials',
  user_inactive: 'errors.userInactive',
  account_locked: 'errors.accountLocked',
  too_many_requests: 'errors.tooManyRequests',
};

const ERROR_CODES = new Set(['invalid_credentials', 'user_inactive', 'account_locked']);

function levelFor(status: number, code: string | undefined): ToastLevel {
  if (status === 0 || status >= 500 || status === 401 || (code !== undefined && ERROR_CODES.has(code))) {
    return ToastLevel.Error;
  }
  return ToastLevel.Warning;
}

function resolveMessage(error: HttpErrorResponse, translate: TranslateService): string {
  const params = { time: formatRetryAfter(apiRetryAfterSeconds(error) ?? 0) };

  for (const code of apiErrorCodes(error)) {
    const key = `errors.identity.${code}`;
    const message = translate.instant(key);
    if (message !== key) return message;
  }

  const code = apiErrorCode(error);
  if (code) {
    const byCode = MESSAGE_KEY_BY_CODE[code] ?? `errors.codes.${code}`;
    const message = translate.instant(byCode, params);
    if (message !== byCode) return message;
  }

  if (error.status === 400 && Object.keys(apiFieldErrors(error)).length > 0) {
    return translate.instant('errors.checkFields');
  }

  return translate.instant(MESSAGE_KEY_BY_STATUS[error.status] ?? 'errors.generic', params);
}

function titleFor(level: ToastLevel, translate: TranslateService): string {
  return translate.instant(`toast.${level}Title`);
}

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const injector = inject(Injector);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && request.url.startsWith(environment.apiUrl)) {
        handle(error, request.method, request.url, request.context.get(SKIP_ERROR_TOAST), injector);
      }

      return throwError(() => error);
    }),
  );
};

function handle(error: HttpErrorResponse, method: string, url: string, skipToast: boolean, injector: Injector): void {
  const auth = injector.get(AuthService);
  const router = injector.get(Router);
  const translate = injector.get(TranslateService);
  const toastr = injector.get(ToastrService);
  const code = apiErrorCode(error);
  const isNavigation = method === 'GET';
  const isCredentialsCall = CREDENTIALS_CALL.test(url);

  const show = (level: ToastLevel, message: string): void => {
    const title = titleFor(level, translate);
    if (level === ToastLevel.Error) toastr.error(message, title);
    else if (level === ToastLevel.Warning) toastr.warning(message, title);
    else toastr.info(message, title);
  };

  if (error.status === 401 && !isCredentialsCall && auth.isAuthenticated()) {
    auth.logout();
    show(ToastLevel.Info, translate.instant('errors.sessionExpired'));
    return;
  }

  if (skipToast || code === 'email_not_confirmed') return;

  if (error.status >= 500) {
    show(ToastLevel.Error, translate.instant('errors.serverError'));
    if (isNavigation) void router.navigate(['/error', '500']);
    return;
  }

  if (error.status === 403 && code === 'forbidden' && isNavigation) {
    void router.navigate(['/error', '403']);
    return;
  }

  if (error.status === 404 && isNavigation) {
    void router.navigate(['/error', '404']);
    return;
  }

  show(levelFor(error.status, code), resolveMessage(error, translate));
}
