import { ChangeDetectionStrategy, Component, computed, inject, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../core/auth/auth.service';
import { BirthdayService } from '../core/birthday/birthday.service';
import { Icon } from '../shared/icon';
import { Toolbar } from '../shared/toolbar';
import { NotificationBell } from './notification-bell';

@Component({
  selector: 'app-navbar',
  imports: [TranslatePipe, Icon, Toolbar, NotificationBell],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  private readonly auth = inject(AuthService);

  protected readonly birthday = inject(BirthdayService);

  readonly menuToggle = output<void>();

  protected readonly userName = computed(() => this.auth.session()?.displayName ?? '');
  protected readonly pictureUrl = computed(() => this.auth.session()?.profilePictureUrl ?? null);
  protected readonly initial = computed(() => this.userName().charAt(0).toUpperCase());

  protected logout(): void {
    this.auth.logout();
  }
}
