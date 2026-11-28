import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../core/auth/auth.service';
import { Permission } from '../../../core/auth/enums/permission.enum';
import { formatMoney } from '../../../core/format/money';
import { LanguageService } from '../../../core/i18n/language.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { Icon } from '../../../shared/icon';
import { Select } from '../../../shared/select/select';
import { SelectOption } from '../../../shared/select/interfaces/select-option.interface';
import { Spinner } from '../../../shared/spinner';
import { SalaryRecord } from '../../../core/salaries/interfaces/salary-record.interface';
import { SalaryService } from '../../../core/salaries/salary.service';

@Component({
  selector: 'app-salary-list',
  imports: [FormsModule, RouterLink, TranslatePipe, Icon, Select, Spinner, ConfirmDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './salary-list.html',
  styleUrl: './salary-list.scss',
})
export class SalaryList {
  private readonly salaries = inject(SalaryService);
  private readonly auth = inject(AuthService);
  private readonly language = inject(LanguageService);
  private readonly translate = inject(TranslateService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly loading = signal(true);
  protected readonly items = signal<SalaryRecord[]>([]);
  protected readonly page = signal(1);
  protected readonly pageSize = 20;
  protected readonly totalCount = signal(0);
  protected readonly yearFilter = signal<string>('');
  protected readonly deletingId = signal<string | null>(null);
  protected readonly confirmOpen = signal(false);

  protected readonly canCreate = computed(() => this.auth.hasPermission(Permission.SalariesCreate));
  protected readonly canUpdate = computed(() => this.auth.hasPermission(Permission.SalariesUpdate));
  protected readonly canDelete = computed(() => this.auth.hasPermission(Permission.SalariesDelete));
  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  protected readonly yearOptions = computed<SelectOption[]>(() => {
    const current = new Date().getFullYear();
    const years = Array.from({ length: current - 1900 + 1 }, (_, index) => current - index);
    return [
      { value: '', label: this.translate.instant('salaries.allYears') },
      ...years.map((year) => ({ value: String(year), label: String(year) })),
    ];
  });

  constructor() {
    this.load();
  }

  protected money(value: number): string {
    return formatMoney(value, this.language.current());
  }

  protected competence(record: SalaryRecord): string {
    const formatter = new Intl.DateTimeFormat(this.language.current(), { month: 'long', year: 'numeric' });
    const label = formatter.format(new Date(record.year, record.month - 1, 1));
    return label.charAt(0).toUpperCase() + label.slice(1);
  }

  protected onYearChange(value: string): void {
    this.yearFilter.set(value);
    this.page.set(1);
    this.load();
  }

  protected previousPage(): void {
    if (this.page() <= 1) return;
    this.page.update((page) => page - 1);
    this.load();
  }

  protected nextPage(): void {
    if (this.page() >= this.totalPages()) return;
    this.page.update((page) => page + 1);
    this.load();
  }

  protected askDelete(id: string): void {
    this.deletingId.set(id);
    this.confirmOpen.set(true);
  }

  protected cancelDelete(): void {
    this.confirmOpen.set(false);
    this.deletingId.set(null);
  }

  protected confirmDelete(): void {
    const id = this.deletingId();
    if (!id) return;
    this.salaries
      .delete(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.confirmOpen.set(false);
          this.deletingId.set(null);
          this.toastr.success(this.translate.instant('toast.salaryDeleted'));
          this.load();
        },
        error: () => {
          this.confirmOpen.set(false);
          this.deletingId.set(null);
        },
      });
  }

  private load(): void {
    this.loading.set(true);
    const year = this.yearFilter() ? Number(this.yearFilter()) : null;
    this.salaries
      .list(year, this.page(), this.pageSize)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.items.set(result.items);
          this.totalCount.set(result.totalCount);
          this.page.set(result.page);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }
}
