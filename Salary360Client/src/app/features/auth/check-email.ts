import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../core/auth/auth.service';
import { Toolbar } from '../../shared/toolbar';

@Component({
  selector: 'app-check-email',
  imports: [RouterLink, TranslatePipe, Toolbar],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './check-email.html',
  styleUrl: './check-email.scss',
})
export class CheckEmail {
  private readonly auth = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly email = inject(ActivatedRoute).snapshot.queryParamMap.get('email') ?? '';
  protected readonly sending = signal(false);

  protected resend(): void {
    this.sending.set(true);
    this.auth
      .resendConfirmation(this.email)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.sending.set(false);
          this.toastr.success(this.translate.instant('toast.confirmationSent'));
        },
        error: () => this.sending.set(false),
      });
  }
}
