import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { environment } from '../../environments/environment';
import { Permission } from '../core/auth/enums/permission.enum';
import { AuthService } from '../core/auth/auth.service';
import { NavItem } from './interfaces/nav-item.interface';
import { Icon } from '../shared/icon';

const NAV_ITEMS: readonly NavItem[] = [
  { labelKey: 'nav.home', route: '/', icon: 'home', exact: true },
  { labelKey: 'nav.salaries', route: '/salaries', icon: 'banknotes', exact: false, permission: Permission.SalariesView },
  { labelKey: 'nav.expenses', route: '/expenses', icon: 'receipt-percent', exact: false, permission: Permission.ExpensesView },
  { labelKey: 'nav.evolution', route: '/evolution', icon: 'chart-bar', exact: false, permission: Permission.SalariesView },
  {
    labelKey: 'nav.salaryIntelligence',
    route: '/salary-intelligence',
    icon: 'sparkles',
    exact: false,
    permission: Permission.SalariesView,
  },
  { labelKey: 'nav.profile', route: '/profile', icon: 'user-circle', exact: false, permission: Permission.ProfileView },
  { labelKey: 'nav.admin', route: '/admin', icon: 'shield-check', exact: false, permission: Permission.RolesView },
];
@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, TranslatePipe, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar {
  private readonly auth = inject(AuthService);

  readonly open = input(false);
  readonly collapsed = input(false);
  readonly closed = output<void>();
  readonly collapseToggle = output<void>();

  protected readonly appName = environment.appName;
  protected readonly items = computed(() =>
    NAV_ITEMS.filter((item) => !item.permission || this.auth.hasPermission(item.permission)),
  );
}
