import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';
import { applyServerErrors } from '../../core/forms/server-errors';
import { birthDateValidator, phoneValidator } from '../../core/validators/profile.validators';
import { todayIsoDate } from '../../core/format/date-format';
import { DatePicker } from '../../shared/date-picker/date-picker';
import { PhoneInput } from '../../shared/phone-input/phone-input';
import { FieldError } from '../../shared/field-error/field-error';
import { Spinner } from '../../shared/spinner';
import { Toolbar } from '../../shared/toolbar';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, Toolbar, Spinner, PhoneInput, DatePicker, FieldError],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly fb = inject(FormBuilder).nonNullable;
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly appName = environment.appName;
  protected readonly loading = signal(false);
  protected readonly today = todayIsoDate();
  protected readonly form = this.fb.group({
    displayName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    dateOfBirth: ['', [Validators.required, birthDateValidator]],
    phoneNumber: ['', [phoneValidator]],
    acceptedTerms: [false, [Validators.requiredTrue]],
    acceptedPrivacyPolicy: [false, [Validators.requiredTrue]],
    password: [
      '',
      [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/)],
    ],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const { phoneNumber, ...rest } = this.form.getRawValue();
    const { email } = rest;
    this.auth.register({ ...rest, phoneNumber: phoneNumber.trim() === '' ? null : phoneNumber.trim() }).subscribe({
      next: () => void this.router.navigate(['/check-email'], { queryParams: { email } }),
      error: (error: unknown) => {
        applyServerErrors(this.form, error);
        this.loading.set(false);
      },
    });
  }
}
