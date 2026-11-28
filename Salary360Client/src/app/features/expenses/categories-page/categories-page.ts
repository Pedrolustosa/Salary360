import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../core/auth/auth.service';
import { Permission } from '../../../core/auth/enums/permission.enum';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { Icon } from '../../../shared/icon';
import { Modal } from '../../../shared/modal/modal';
import { Select } from '../../../shared/select/select';
import { SelectOption } from '../../../shared/select/interfaces/select-option.interface';
import { Skeleton } from '../../../shared/skeleton/skeleton';
import { CategoryFormDialog } from '../category-form-dialog/category-form-dialog';
import { ExpenseCategoryListItem } from '../interfaces/expense-category-list-item.interface';
import { ExpenseCategoryService } from '../services/expense-category.service';
import { categoryColorClasses } from '../../../shared/utils/category-color-classes';
import { categoryDisplayName } from '../utils/category-display';
import { categoryIconName } from '../utils/category-icon-name';

@Component({
  selector: 'app-categories-page',
  imports: [
    FormsModule,
    RouterLink,
    TranslatePipe,
    Icon,
    Skeleton,
    ConfirmDialog,
    Modal,
    Select,
    CategoryFormDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './categories-page.html',
  styleUrl: './categories-page.scss',
})
export class CategoriesPage {
  private readonly api = inject(ExpenseCategoryService);
  private readonly auth = inject(AuthService);
  private readonly translate = inject(TranslateService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly categoryIconName = categoryIconName;
  protected readonly categoryColorClasses = categoryColorClasses;

  protected readonly loading = signal(true);
  protected readonly error = signal(false);
  protected readonly items = signal<ExpenseCategoryListItem[]>([]);
  protected readonly creatingDefaults = signal(false);

  protected readonly formOpen = signal(false);
  protected readonly editing = signal<ExpenseCategoryListItem | null>(null);
  protected readonly confirmOpen = signal(false);
  protected readonly reassignOpen = signal(false);
  protected readonly deleting = signal<ExpenseCategoryListItem | null>(null);
  protected readonly reassignTo = signal('');

  protected readonly canCreate = computed(() => this.auth.hasPermission(Permission.ExpensesCreate));
  protected readonly canUpdate = computed(() => this.auth.hasPermission(Permission.ExpensesUpdate));
  protected readonly canDelete = computed(() => this.auth.hasPermission(Permission.ExpensesDelete));

  protected readonly reassignOptions = computed<SelectOption[]>(() => {
    const current = this.deleting();
    return this.items()
      .filter((item) => item.id !== current?.id)
      .map((item) => ({
        value: item.id,
        label: categoryDisplayName(item.name, item.systemKey, (key) => this.translate.instant(key)),
        dotClass: categoryColorClasses(item.colorKey).bg,
      }));
  });

  constructor() {
    this.load();
    this.api.changed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  protected displayName(item: ExpenseCategoryListItem): string {
    return categoryDisplayName(item.name, item.systemKey, (key) => this.translate.instant(key));
  }

  protected openCreate(): void {
    this.editing.set(null);
    this.formOpen.set(true);
  }

  protected openEdit(item: ExpenseCategoryListItem): void {
    this.editing.set(item);
    this.formOpen.set(true);
  }

  protected closeForm(): void {
    this.formOpen.set(false);
    this.editing.set(null);
  }

  protected onSaved(): void {
    this.load();
  }

  protected askDelete(item: ExpenseCategoryListItem): void {
    this.deleting.set(item);
    this.reassignTo.set('');
    if (item.expenseCount > 0) {
      this.reassignOpen.set(true);
    } else {
      this.confirmOpen.set(true);
    }
  }

  protected cancelDelete(): void {
    this.confirmOpen.set(false);
    this.reassignOpen.set(false);
    this.deleting.set(null);
    this.reassignTo.set('');
  }

  protected confirmDelete(): void {
    const item = this.deleting();
    if (!item) return;
    const reassignTo = item.expenseCount > 0 ? this.reassignTo() || null : null;
    if (item.expenseCount > 0 && !reassignTo) return;

    this.api
      .delete(item.id, reassignTo)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.toastr.success(this.translate.instant('toast.categoryDeleted'));
          this.load();
        },
        error: () => this.cancelDelete(),
      });
  }

  protected createDefaults(): void {
    if (!this.canCreate() || this.creatingDefaults()) return;
    this.creatingDefaults.set(true);
    this.api
      .createDefaults()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.creatingDefaults.set(false);
          this.toastr.success(this.translate.instant('toast.defaultsCreated', { created: result.created }));
          this.load();
        },
        error: () => this.creatingDefaults.set(false),
      });
  }

  protected retry(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(false);
    this.api
      .list()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => {
          this.items.set(items);
          this.loading.set(false);
        },
        error: () => {
          this.items.set([]);
          this.loading.set(false);
          this.error.set(true);
        },
      });
  }
}
