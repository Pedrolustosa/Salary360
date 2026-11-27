import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../core/auth/auth.service';
import { BirthdayService } from '../core/birthday/birthday.service';
import { Icon } from '../shared/icon';
import { ConfettiPiece } from './interfaces/confetti-piece.interface';

const CONFETTI_COUNT = 70;
const CONFETTI_COLORS = ['#10b981', '#0ea5e9', '#8b5cf6', '#f59e0b', '#ec4899', '#ef4444'];
const AUTO_CLOSE_MS = 12000;

function createConfetti(): ConfettiPiece[] {
  return Array.from({ length: CONFETTI_COUNT }, (_, index) => ({
    left: Math.random() * 100,
    size: 6 + Math.random() * 8,
    delay: Math.random() * 3,
    duration: 3.5 + Math.random() * 3,
    color: CONFETTI_COLORS[index % CONFETTI_COLORS.length],
    round: index % 3 === 0,
  }));
}

@Component({
  selector: 'app-birthday-celebration',
  imports: [TranslatePipe, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './birthday-celebration.html',
  styleUrl: './birthday-celebration.scss',
})
export class BirthdayCelebration {
  private readonly auth = inject(AuthService);
  private autoCloseTimer?: ReturnType<typeof setTimeout>;

  protected readonly birthday = inject(BirthdayService);
  protected readonly confetti = createConfetti();
  protected readonly firstName = computed(() => (this.auth.session()?.displayName ?? '').split(' ')[0]);

  constructor() {
    this.autoCloseTimer = setTimeout(() => this.birthday.dismiss(), AUTO_CLOSE_MS);
    inject(DestroyRef).onDestroy(() => clearTimeout(this.autoCloseTimer));
  }
}
