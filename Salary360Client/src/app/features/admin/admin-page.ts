import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { forkJoin, of } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { PERMISSION_ACTIONS, PERMISSION_RESOURCES } from '../../core/auth/constants/permission-matrix';
import { Permission } from '../../core/auth/enums/permission.enum';
import { Role } from '../../core/auth/enums/role.enum';
import { RoleSummary } from '../../core/auth/interfaces/role-summary.interface';
import { Spinner } from '../../shared/spinner';
import { PermissionRow } from './interfaces/permission-row.interface';

const PERMISSION_ROWS: PermissionRow[] = PERMISSION_RESOURCES.flatMap((resource) =>
  PERMISSION_ACTIONS.filter((action) => Object.values(Permission).includes(`${resource}.${action}` as never)).map(
    (action, index) => ({ permission: `${resource}.${action}`, resource, action, firstOfResource: index === 0 }),
  ),
);

@Component({
  selector: 'app-admin-page',
  imports: [TranslatePipe, Spinner],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './admin-page.html',
  styleUrl: './admin-page.scss',
})
export class AdminPage {
  private readonly auth = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly roles = signal<RoleSummary[]>([]);
  protected readonly draft = signal<Record<string, string[]>>({});
  protected readonly rows = PERMISSION_ROWS;
  protected readonly canEdit = computed(() => this.auth.hasPermission(Permission.RolesUpdate));
  protected readonly dirtyRoles = computed(() =>
    this.roles().filter((role) => !this.sameSet(role.permissions, this.draft()[role.name] ?? role.permissions)),
  );

  constructor() {
    this.load();
  }

  protected isLocked(role: RoleSummary): boolean {
    return !this.canEdit() || role.name === Role.Admin;
  }

  protected granted(role: RoleSummary, permission: string): boolean {
    return (this.draft()[role.name] ?? role.permissions).includes(permission);
  }

  protected toggle(role: RoleSummary, permission: string, checked: boolean): void {
    if (this.isLocked(role)) return;
    const current = this.draft()[role.name] ?? role.permissions;
    const next = checked ? [...new Set([...current, permission])] : current.filter((item) => item !== permission);
    this.draft.update((draft) => ({ ...draft, [role.name]: next }));
  }

  protected discard(): void {
    this.draft.set(this.toDraft(this.roles()));
  }

  protected save(): void {
    const changed = this.dirtyRoles();
    if (changed.length === 0) return;

    this.saving.set(true);
    forkJoin(changed.map((role) => this.auth.updateRolePermissions(role.name, this.draft()[role.name])))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.roles.update((roles) => roles.map((role) => updated.find((item) => item.name === role.name) ?? role));
          this.discard();
          this.saving.set(false);
          this.toastr.success(this.translate.instant('admin.saved'));
        },
        error: () => this.saving.set(false),
      });
  }

  private load(): void {
    forkJoin({ roles: this.auth.hasPermission(Permission.RolesView) ? this.auth.listRoles() : of<RoleSummary[]>([]) })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ roles }) => {
          this.roles.set(roles);
          this.draft.set(this.toDraft(roles));
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  private toDraft(roles: RoleSummary[]): Record<string, string[]> {
    return Object.fromEntries(roles.map((role) => [role.name, [...role.permissions]]));
  }

  private sameSet(a: string[], b: string[]): boolean {
    return a.length === b.length && a.every((item) => b.includes(item));
  }
}
