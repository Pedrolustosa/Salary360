import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { Permission } from '../../core/auth/enums/permission.enum';
import { UserStatus } from '../../core/auth/enums/user-status.enum';
import { Role } from '../../core/auth/enums/role.enum';
import { UserProfile } from '../../core/auth/interfaces/user-profile.interface';
import { formatDateTime } from '../../core/format/date-format';
import { formatPhone } from '../../core/format/phone-format';
import { LanguageService } from '../../core/i18n/language.service';
import { NotificationService } from '../../core/notifications/notification.service';
import { DoughnutChart } from '../../shared/doughnut-chart';
import { Icon } from '../../shared/icon';
import { StatTone } from '../../shared/stat-card/enums/stat-tone.enum';
import { StatCard } from '../../shared/stat-card/stat-card';
import { Spinner } from '../../shared/spinner';

const SAMPLE_KEYS = ['home.sample.a', 'home.sample.b', 'home.sample.c'];

@Component({
  selector: 'app-home',
  imports: [RouterLink, TranslatePipe, DoughnutChart, Icon, Spinner, StatCard],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private readonly auth = inject(AuthService);
  private readonly translate = inject(TranslateService);
  private readonly notifications = inject(NotificationService);
  private readonly language = inject(LanguageService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly sending = signal(false);
  protected readonly usersLoading = signal(false);
  protected readonly users = signal<UserProfile[]>([]);
  protected readonly canViewUsers = computed(() => this.auth.hasPermission(Permission.UsersView));
  protected readonly UserStatus = UserStatus;
  protected readonly StatTone = StatTone;
  protected readonly canUpdateUsers = computed(() => this.auth.hasPermission(Permission.UsersUpdate));
  protected readonly currentUserId = computed(() => this.auth.session()?.userId ?? '');
  protected readonly canViewRoles = computed(() => this.auth.hasPermission(Permission.RolesView));
  protected readonly userName = computed(() => this.auth.session()?.displayName ?? '');
  protected readonly totalUsers = computed(() => this.users().length);
  protected readonly activeUsers = computed(() => this.users().filter((user) => user.status === UserStatus.Active).length);
  protected readonly inactiveUsers = computed(() => this.totalUsers() - this.activeUsers());
  protected readonly adminUsers = computed(() => this.users().filter((user) => user.roles.includes(Role.Admin)).length);

  protected percentOfTotal(count: number): number {
    return this.totalUsers() === 0 ? 0 : Math.round((count / this.totalUsers()) * 100);
  }

  protected readonly sampleValues = [50, 30, 20];
  private readonly labels = toSignal(this.translate.stream(SAMPLE_KEYS), {
    initialValue: {} as Record<string, string>,
  });
  protected readonly sampleLabels = computed(() => SAMPLE_KEYS.map((key) => this.labels()[key] ?? key));

  constructor() {
    if (!this.canViewUsers()) return;
    this.usersLoading.set(true);
    this.auth
      .listUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => {
          this.users.set(users);
          this.usersLoading.set(false);
        },
        error: () => this.usersLoading.set(false),
      });
  }

  protected phone(user: UserProfile): string {
    return formatPhone(user.phoneNumber);
  }

  protected dateTime(value: string | null): string {
    return formatDateTime(value, this.language.current()) || '-';
  }

  protected toggleStatus(user: UserProfile): void {
    const next = user.status === UserStatus.Active ? UserStatus.Inactive : UserStatus.Active;
    this.auth
      .setUserStatus(user.userId, next)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((updated) => {
        this.users.update((users) => users.map((item) => (item.userId === updated.userId ? updated : item)));
      });
  }

  protected sendTestNotification(): void {
    this.sending.set(true);
    this.notifications
      .sendTest()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.sending.set(false),
        error: () => this.sending.set(false),
      });
  }
}
