import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { Subject, catchError, debounceTime, merge, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { Permission } from '../../../core/auth/enums/permission.enum';
import { formatMoney } from '../../../core/format/money';
import { LanguageService } from '../../../core/i18n/language.service';
import { BarChart } from '../../../shared/bar-chart/bar-chart';
import { BarChartDataset } from '../../../shared/bar-chart/interfaces/bar-chart-dataset.interface';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { DoughnutChart } from '../../../shared/doughnut-chart';
import { Icon } from '../../../shared/icon';
import { Modal } from '../../../shared/modal/modal';
import { MonthYearPicker } from '../../../shared/month-year-picker/month-year-picker';
import { MonthYearValue } from '../../../shared/month-year-picker/interfaces/month-year-value.interface';
import { Select } from '../../../shared/select/select';
import { SelectOption } from '../../../shared/select/interfaces/select-option.interface';
import { Skeleton } from '../../../shared/skeleton/skeleton';
import { StatCard } from '../../../shared/stat-card/stat-card';
import { StatTone } from '../../../shared/stat-card/enums/stat-tone.enum';
import { SegmentedControl } from '../../../shared/segmented-control/segmented-control';
import { SegmentedOption } from '../../../shared/segmented-control/interfaces/segmented-option.interface';
import { SalaryService } from '../../salaries/services/salary.service';
import { ExpenseFormDialog } from '../expense-form-dialog/expense-form-dialog';
import { ExpenseChangeScope } from '../enums/expense-change-scope.enum';
import { ExpenseType } from '../enums/expense-type.enum';
import { ExpenseCategoryListItem } from '../interfaces/expense-category-list-item.interface';
import { ExpenseListItem } from '../interfaces/expense-list-item.interface';
import { ExpenseMonthSummary } from '../interfaces/expense-month-summary.interface';
import { ExpenseSeriesPoint } from '../interfaces/expense-series-point.interface';
import { ExpenseCategoryService } from '../services/expense-category.service';
import { ExpenseService } from '../services/expense.service';
import { categoryColorClasses } from '../utils/category-color-classes';
import { categoryDisplayName } from '../utils/category-display';
import { categoryIconName } from '../utils/category-icon-name';
import { seriesFromMonths, shiftCompetence } from '../utils/competence';
import { isExpenseOverdue } from '../utils/expense-overdue';

@Component({
  selector: 'app-expenses-page',
  imports: [
    FormsModule,
    RouterLink,
    TranslatePipe,
    Icon,
    Select,
    MonthYearPicker,
    StatCard,
    DoughnutChart,
    BarChart,
    Skeleton,
    ConfirmDialog,
    Modal,
    SegmentedControl,
    ExpenseFormDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './expenses-page.html',
  styleUrl: './expenses-page.scss',
})
export class ExpensesPage {
  private readonly expenses = inject(ExpenseService);
  private readonly categoriesApi = inject(ExpenseCategoryService);
  private readonly salaries = inject(SalaryService);
  private readonly auth = inject(AuthService);
  private readonly language = inject(LanguageService);
  private readonly translate = inject(TranslateService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly reload$ = new Subject<void>();

  protected readonly StatTone = StatTone;
  protected readonly ExpenseType = ExpenseType;
  protected readonly categoryIconName = categoryIconName;
  protected readonly categoryColorClasses = categoryColorClasses;
  protected readonly isOverdue = isExpenseOverdue;

  private readonly now = new Date();
  protected readonly competence = signal<MonthYearValue>({
    year: this.now.getFullYear(),
    month: this.now.getMonth() + 1,
  });
  protected readonly page = signal(1);
  protected readonly pageSize = 20;
  protected readonly categoryFilter = signal('');
  protected readonly typeFilter = signal('');
  protected readonly paidFilter = signal('');

  protected readonly categories = signal<ExpenseCategoryListItem[]>([]);
  protected readonly summary = signal<ExpenseMonthSummary | null>(null);
  protected readonly summaryLoading = signal(true);
  protected readonly summaryError = signal(false);
  protected readonly series = signal<ExpenseSeriesPoint[]>([]);
  protected readonly seriesLoading = signal(true);
  protected readonly seriesError = signal(false);
  protected readonly items = signal<ExpenseListItem[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly listLoading = signal(true);
  protected readonly listError = signal(false);

  protected readonly formOpen = signal(false);
  protected readonly editingId = signal<string | null>(null);
  protected readonly confirmOpen = signal(false);
  protected readonly deleting = signal<ExpenseListItem | null>(null);
  protected readonly deleteScope = signal(ExpenseChangeScope.This);
  protected readonly scopeModalOpen = signal(false);
  protected readonly copying = signal(false);

  protected readonly scopeOptions: SegmentedOption[] = [
    { value: ExpenseChangeScope.This, labelKey: 'expenses.scope.This' },
    { value: ExpenseChangeScope.ThisAndFollowing, labelKey: 'expenses.scope.ThisAndFollowing' },
    { value: ExpenseChangeScope.All, labelKey: 'expenses.scope.All' },
  ];

  protected readonly canCreate = computed(() => this.auth.hasPermission(Permission.ExpensesCreate));
  protected readonly canUpdate = computed(() => this.auth.hasPermission(Permission.ExpensesUpdate));
  protected readonly canDelete = computed(() => this.auth.hasPermission(Permission.ExpensesDelete));
  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  protected readonly categoryFilterOptions = computed<SelectOption[]>(() => [
    { value: '', label: this.translate.instant('expenses.allCategories') },
    ...this.categories().map((category) => ({
      value: category.id,
      label: categoryDisplayName(category.name, category.systemKey, (key) => this.translate.instant(key)),
      dotClass: categoryColorClasses(category.colorKey).bg,
    })),
  ]);

  protected readonly typeFilterOptions = computed<SelectOption[]>(() => [
    { value: '', label: this.translate.instant('expenses.allTypes') },
    { value: ExpenseType.Fixed, label: this.translate.instant('expenses.type.Fixed') },
    { value: ExpenseType.Variable, label: this.translate.instant('expenses.type.Variable') },
  ]);

  protected readonly paidFilterOptions = computed<SelectOption[]>(() => [
    { value: '', label: this.translate.instant('expenses.allPaidStatus') },
    { value: 'true', label: this.translate.instant('expenses.paidOnly') },
    { value: 'false', label: this.translate.instant('expenses.unpaidOnly') },
  ]);

  protected readonly netDisplay = computed(() => {
    const summary = this.summary();
    if (!summary || !summary.hasSalary || summary.netAmount == null) return '—';
    return formatMoney(summary.netAmount, this.language.current());
  });

  protected readonly totalDisplay = computed(() => {
    const summary = this.summary();
    if (!summary) return '—';
    return formatMoney(summary.total, this.language.current());
  });

  protected readonly committedDisplay = computed(() => {
    const summary = this.summary();
    if (!summary || summary.committedPercent == null) return '—';
    return `${new Intl.NumberFormat(this.language.current(), { maximumFractionDigits: 1 }).format(summary.committedPercent)}%`;
  });

  protected readonly availableDisplay = computed(() => {
    const summary = this.summary();
    if (!summary || !summary.hasSalary || summary.available == null) return '—';
    return formatMoney(summary.available, this.language.current());
  });

  protected readonly availableTone = computed(() => {
    const available = this.summary()?.available;
    return available != null && available < 0 ? StatTone.Rose : StatTone.Emerald;
  });

  protected readonly doughnutLabels = computed(() =>
    (this.summary()?.byCategory ?? []).map((item) =>
      categoryDisplayName(item.name, item.systemKey, (key) => this.translate.instant(key)),
    ),
  );

  protected readonly doughnutValues = computed(() => (this.summary()?.byCategory ?? []).map((item) => item.total));

  protected readonly doughnutColors = computed(() =>
    (this.summary()?.byCategory ?? []).map((item) => categoryColorClasses(item.colorKey).chart),
  );

  protected readonly barLabels = computed(() => {
    const formatter = new Intl.DateTimeFormat(this.language.current(), { month: 'short', year: '2-digit' });
    return this.series().map((point) => {
      const label = formatter.format(new Date(point.year, point.month - 1, 1));
      return label.charAt(0).toUpperCase() + label.slice(1);
    });
  });

  protected readonly barDatasets = computed<BarChartDataset[]>(() => [
    {
      label: this.translate.instant('expenses.type.Fixed'),
      data: this.series().map((point) => point.totalFixed),
      color: '#0ea5e9',
    },
    {
      label: this.translate.instant('expenses.type.Variable'),
      data: this.series().map((point) => point.totalVariable),
      color: '#f59e0b',
    },
  ]);

  protected readonly yFormatter = (value: number): string => formatMoney(value, this.language.current());

  private readonly listQuery = computed(() => ({
    year: this.competence().year,
    month: this.competence().month,
    categoryId: this.categoryFilter() || null,
    type: (this.typeFilter() || null) as ExpenseType | null,
    isPaid: this.paidFilter() === '' ? null : this.paidFilter() === 'true',
    page: this.page(),
    pageSize: this.pageSize,
  }));

  constructor() {
    this.loadCategories();

    const competence$ = merge(
      toObservable(this.competence),
      this.reload$,
      this.expenses.changed$,
      this.salaries.changed$,
    ).pipe(debounceTime(200));

    competence$
      .pipe(
        tap(() => {
          this.summaryLoading.set(true);
          this.summaryError.set(false);
        }),
        switchMap(() => {
          const { year, month } = this.competence();
          return this.expenses.summary(year, month).pipe(
            tap((data) => {
              this.summary.set(data);
              this.summaryLoading.set(false);
            }),
            catchError(() => {
              this.summary.set(null);
              this.summaryLoading.set(false);
              this.summaryError.set(true);
              return of(null);
            }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();

    competence$
      .pipe(
        tap(() => {
          this.seriesLoading.set(true);
          this.seriesError.set(false);
        }),
        switchMap(() => {
          const range = seriesFromMonths(this.competence(), 12);
          return this.expenses.series(range.from.year, range.from.month, range.to.year, range.to.month).pipe(
            tap((data) => {
              this.series.set(data);
              this.seriesLoading.set(false);
            }),
            catchError(() => {
              this.series.set([]);
              this.seriesLoading.set(false);
              this.seriesError.set(true);
              return of([]);
            }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();

    merge(toObservable(this.listQuery), this.reload$, this.expenses.changed$, this.salaries.changed$)
      .pipe(
        debounceTime(200),
        tap(() => {
          this.listLoading.set(true);
          this.listError.set(false);
        }),
        switchMap(() =>
          this.expenses.list(this.listQuery()).pipe(
            tap((result) => {
              this.items.set(result.items);
              this.totalCount.set(result.total);
              this.page.set(result.page);
              this.listLoading.set(false);
            }),
            catchError(() => {
              this.items.set([]);
              this.totalCount.set(0);
              this.listLoading.set(false);
              this.listError.set(true);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();

    this.categoriesApi.changed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.loadCategories());
  }

  protected money(value: number): string {
    return formatMoney(value, this.language.current());
  }

  protected categoryLabel(item: ExpenseListItem): string {
    return categoryDisplayName(item.categoryName, item.categorySystemKey, (key) => this.translate.instant(key));
  }

  protected previousMonth(): void {
    this.competence.set(shiftCompetence(this.competence(), -1));
    this.page.set(1);
  }

  protected nextMonth(): void {
    const next = shiftCompetence(this.competence(), 1);
    const max = { year: this.now.getFullYear(), month: this.now.getMonth() + 1 };
    if (next.year > max.year || (next.year === max.year && next.month > max.month)) return;
    this.competence.set(next);
    this.page.set(1);
  }

  protected onCompetenceChange(value: MonthYearValue | null): void {
    if (!value) return;
    this.competence.set(value);
    this.page.set(1);
  }

  protected onCategoryFilter(value: string): void {
    this.categoryFilter.set(value);
    this.page.set(1);
  }

  protected onTypeFilter(value: string): void {
    this.typeFilter.set(value);
    this.page.set(1);
  }

  protected onPaidFilter(value: string): void {
    this.paidFilter.set(value);
    this.page.set(1);
  }

  protected previousPage(): void {
    if (this.page() <= 1) return;
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    if (this.page() >= this.totalPages()) return;
    this.page.update((page) => page + 1);
  }

  protected openCreate(): void {
    this.editingId.set(null);
    this.formOpen.set(true);
  }

  protected openEdit(id: string): void {
    this.editingId.set(id);
    this.formOpen.set(true);
  }

  protected closeForm(): void {
    this.formOpen.set(false);
    this.editingId.set(null);
  }

  protected onFormSaved(): void {
    this.reload$.next();
  }

  protected onCategoriesChanged(): void {
    this.loadCategories();
  }

  protected togglePaid(item: ExpenseListItem): void {
    if (!this.canUpdate()) return;
    this.expenses
      .setPaid(item.id, !item.isPaid)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.reload$.next(),
      });
  }

  protected askDelete(item: ExpenseListItem): void {
    this.deleting.set(item);
    this.deleteScope.set(ExpenseChangeScope.This);
    if (item.seriesId) {
      this.scopeModalOpen.set(true);
    } else {
      this.confirmOpen.set(true);
    }
  }

  protected cancelDelete(): void {
    this.confirmOpen.set(false);
    this.scopeModalOpen.set(false);
    this.deleting.set(null);
  }

  protected onDeleteScopeChange(value: string): void {
    this.deleteScope.set(value as ExpenseChangeScope);
  }

  protected confirmDelete(): void {
    const item = this.deleting();
    if (!item) return;
    this.expenses
      .delete(item.id, this.deleteScope())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.toastr.success(this.translate.instant('toast.expenseDeleted'));
          this.reload$.next();
        },
        error: () => this.cancelDelete(),
      });
  }

  protected copyPreviousMonth(): void {
    if (!this.canCreate() || this.copying()) return;
    const current = this.competence();
    const previous = shiftCompetence(current, -1);
    this.copying.set(true);
    this.expenses
      .copy({
        fromYear: previous.year,
        fromMonth: previous.month,
        toYear: current.year,
        toMonth: current.month,
        onlyFixed: true,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.copying.set(false);
          this.toastr.success(
            this.translate.instant('toast.expensesCopied', { created: result.created, skipped: result.skipped }),
          );
          this.reload$.next();
        },
        error: () => this.copying.set(false),
      });
  }

  protected retrySummary(): void {
    this.reload$.next();
  }

  protected retrySeries(): void {
    this.reload$.next();
  }

  protected retryList(): void {
    this.reload$.next();
  }

  private loadCategories(): void {
    this.categoriesApi
      .list()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => this.categories.set(items),
        error: () => this.categories.set([]),
      });
  }
}
