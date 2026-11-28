import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { Spinner } from '../../shared/spinner';
import { Toolbar } from '../../shared/toolbar';
import { ConfirmationState } from './enums/confirmation-state.enum';

@Component({
  selector: 'app-confirm-email',
  imports: [RouterLink, TranslatePipe, Toolbar, Spinner],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './confirm-email.html',
  styleUrl: './confirm-email.scss',
})
export class ConfirmEmail {
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly ConfirmationState = ConfirmationState;
  protected readonly state = signal<ConfirmationState>(ConfirmationState.Loading);

  constructor() {
    const params = inject(ActivatedRoute).snapshot.queryParamMap;
    const userId = params.get('userId');
    const token = params.get('token');

    if (!userId || !token) {
      this.state.set(ConfirmationState.Error);
      return;
    }

    this.auth
      .confirmEmail(userId, token)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.state.set(ConfirmationState.Success),
        error: () => this.state.set(ConfirmationState.Error),
      });
  }
}
