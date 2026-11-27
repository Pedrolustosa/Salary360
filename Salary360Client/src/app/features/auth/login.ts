import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { environment } from '../../../environments/environment';
import { apiErrorCode, apiRetryAfterSeconds, formatRetryAfter } from '../../core/api/api-error';
import { AuthService } from '../../core/auth/auth.service';
import { applyServerErrors } from '../../core/forms/server-errors';
import { FieldError } from '../../shared/field-error/field-error';
import { Spinner } from '../../shared/spinner';
import { Toolbar } from '../../shared/toolbar';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, Toolbar, Spinner, FieldError],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly fb = inject(FormBuilder).nonNullable;
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private countdownTimer: ReturnType<typeof setInterval> | null = null;

  protected readonly appName = environment.appName;
  protected readonly loading = signal(false);
  protected readonly unconfirmedEmail = signal<string | null>(null);
  protected readonly invalidCredentials = signal(false);
  protected readonly blockedSeconds = signal(0);
  protected readonly blockedTime = computed(() => formatRetryAfter(this.blockedSeconds()));
  protected readonly blocked = computed(() => this.blockedSeconds() > 0);
  private readonly rememberedEmail = this.auth.rememberedEmail();

  protected readonly form = this.fb.group({
    email: [this.rememberedEmail, [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    remember: [this.rememberedEmail !== ''],
  });

  constructor() {
    this.destroyRef.onDestroy(() => this.stopCountdown());
  }

  protected submit(): void {
    if (this.blocked()) return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.unconfirmedEmail.set(null);
    this.invalidCredentials.set(false);
    const { email, password, remember } = this.form.getRawValue();
    this.auth.login({ email, password }, remember).subscribe({
      next: () => void this.router.navigate(['/']),
      error: (error: unknown) => {
        this.loading.set(false);
        const code = apiErrorCode(error);
        if (code === 'email_not_confirmed') this.unconfirmedEmail.set(email);
        if (code === 'invalid_credentials') this.invalidCredentials.set(true);
        if (code === 'too_many_requests' || code === 'account_locked') {
          const seconds = apiRetryAfterSeconds(error);
          if (seconds !== null) this.startCountdown(seconds);
        }
        applyServerErrors(this.form, error);
      },
    });
  }

  private startCountdown(seconds: number): void {
    this.stopCountdown();
    this.blockedSeconds.set(Math.ceil(seconds));
    this.countdownTimer = setInterval(() => {
      const remaining = this.blockedSeconds() - 1;
      this.blockedSeconds.set(Math.max(0, remaining));
      if (remaining <= 0) this.stopCountdown();
    }, 1000);
  }

  private stopCountdown(): void {
    if (this.countdownTimer !== null) {
      clearInterval(this.countdownTimer);
      this.countdownTimer = null;
    }
  }
}
