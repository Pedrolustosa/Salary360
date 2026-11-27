import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { Spinner } from '../../shared/spinner';
import { Toolbar } from '../../shared/toolbar';

@Component({
  selector: 'app-resend-confirmation',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, Toolbar, Spinner],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './resend-confirmation.html',
  styleUrl: './resend-confirmation.scss',
})
export class ResendConfirmation {
  private readonly auth = inject(AuthService);

  protected readonly loading = signal(false);
  protected readonly done = signal(false);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: [inject(ActivatedRoute).snapshot.queryParamMap.get('email') ?? '', [Validators.required, Validators.email]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.auth.resendConfirmation(this.form.getRawValue().email).subscribe({
      next: () => {
        this.loading.set(false);
        this.done.set(true);
      },
      error: () => this.loading.set(false),
    });
  }
}
