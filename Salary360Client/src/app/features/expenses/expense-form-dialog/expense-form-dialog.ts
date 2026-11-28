import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { startWith } from 'rxjs';
import { apiErrorCode } from '../../../core/api/api-error';
import { applyServerErrors } from '../../../core/forms/server-errors';
import { LanguageService } from '../../../core/i18n/language.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { FieldError } from '../../../shared/field-error/field-error';
import { Modal } from '../../../shared/modal/modal';
import { MoneyInput } from '../../../shared/money-input/money-input';
import { MonthYearPicker } from '../../../shared/month-year-picker/month-year-picker';
import { MonthYearValue } from '../../../shared/month-year-picker/interfaces/month-year-value.interface';
import { SegmentedControl } from '../../../shared/segmented-control/segmented-control';
import { SegmentedOption } from '../../../shared/segmented-control/interfaces/segmented-option.interface';
import { Select } from '../../../shared/select/select';
import { SelectOption } from '../../../shared/select/interfaces/select-option.interface';
import { ToggleSwitch } from '../../../shared/toggle-switch/toggle-switch';
import { CategoryFormDialog } from '../category-form-dialog/category-form-dialog';
import { ExpenseChangeScope } from '../enums/expense-change-scope.enum';
import { ExpenseRecurrence } from '../enums/expense-recurrence.enum';
import { ExpenseType } from '../enums/expense-type.enum';
import { Expense } from '../interfaces/expense.interface';
import { ExpenseCategoryListItem } from '../interfaces/expense-category-list-item.interface';
import { ExpenseService } from '../services/expense.service';
import { categoryColorClasses } from '../../../shared/utils/category-color-classes';
import { categoryDisplayName } from '../utils/category-display';
import { competenceRangePreview } from '../utils/competence';

const MAX_NAME = 100;
const MAX_NOTES = 500;
const CREATE_CATEGORY_VALUE = '__create_category__';

@Component({
  selector: 'app-expense-form-dialog',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    Modal,
    MoneyInput,
    MonthYearPicker,
    Select,
    SegmentedControl,
    ToggleSwitch,
    FieldError,
    ConfirmDialog,
    CategoryFormDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './expense-form-dialog.html',
  styleUrl: './expense-form-dialog.scss',
})
export class ExpenseFormDialog {
  private readonly fb = inject(FormBuilder);
  private readonly expenses = inject(ExpenseService);
  private readonly translate = inject(TranslateService);
  private readonly toastr = inject(ToastrService);
  private readonly language = inject(LanguageService);
  private readonly destroyRef = inject(DestroyRef);

  readonly open = input(false);
  readonly expenseId = input<string | null>(null);
  readonly competence = input.required<MonthYearValue>();
  readonly categories = input.required<ExpenseCategoryListItem[]>();

  readonly closed = output<void>();
  readonly saved = output<void>();
  readonly categoriesChanged = output<void>();

  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly discardOpen = signal(false);
  protected readonly categoryDialogOpen = signal(false);
  protected readonly editing = signal<Expense | null>(null);
  protected readonly MaxName = MAX_NAME;
  protected readonly MaxNotes = MAX_NOTES;
  protected readonly CREATE_CATEGORY_VALUE = CREATE_CATEGORY_VALUE;

  protected readonly form = this.fb.group({
    name: this.fb.control('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(MAX_NAME)] }),
    categoryId: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
    type: this.fb.control<ExpenseType>(ExpenseType.Variable, { nonNullable: true, validators: [Validators.required] }),
    amount: this.fb.control<number | null>(null, { validators: [Validators.required, Validators.min(0.01)] }),
    competence: this.fb.control<MonthYearValue | null>(null, { validators: [Validators.required] }),
    dueDay: this.fb.control('', { nonNullable: true }),
    isPaid: this.fb.control(false, { nonNullable: true }),
    notes: this.fb.control('', { nonNullable: true, validators: [Validators.maxLength(MAX_NOTES)] }),
    recurrenceEnabled: this.fb.control(false, { nonNullable: true }),
    recurrenceCount: this.fb.control('2', { nonNullable: true }),
    scope: this.fb.control(ExpenseChangeScope.This, { nonNullable: true }),
  });

  private readonly formValue = toSignal(this.form.valueChanges.pipe(startWith(this.form.getRawValue())), {
    initialValue: this.form.getRawValue(),
  });

  protected readonly typeOptions: SegmentedOption[] = [
    { value: ExpenseType.Fixed, labelKey: 'expenses.type.Fixed' },
    { value: ExpenseType.Variable, labelKey: 'expenses.type.Variable' },
  ];

  protected readonly scopeOptions: SegmentedOption[] = [
    { value: ExpenseChangeScope.This, labelKey: 'expenses.scope.This' },
    { value: ExpenseChangeScope.ThisAndFollowing, labelKey: 'expenses.scope.ThisAndFollowing' },
    { value: ExpenseChangeScope.All, labelKey: 'expenses.scope.All' },
  ];

  protected readonly dueDayOptions = computed<SelectOption[]>(() => [
    { value: '', label: this.translate.instant('expenses.noDueDay') },
    ...Array.from({ length: 31 }, (_, index) => ({
      value: String(index + 1),
      label: String(index + 1),
    })),
  ]);

  protected readonly recurrenceCountOptions = computed<SelectOption[]>(() =>
    Array.from({ length: 59 }, (_, index) => {
      const value = index + 2;
      return { value: String(value), label: String(value) };
    }),
  );

  protected readonly categoryOptions = computed<SelectOption[]>(() => {
    const items = this.categories().map((category) => ({
      value: category.id,
      label: categoryDisplayName(category.name, category.systemKey, (key) => this.translate.instant(key)),
      dotClass: categoryColorClasses(category.colorKey).bg,
    }));
    return [
      ...items,
      {
        value: CREATE_CATEGORY_VALUE,
        label: this.translate.instant('expenses.createCategoryOption'),
        textClass: 'text-brand-600 dark:text-brand-400 font-medium',
      },
    ];
  });

  protected readonly title = computed(() =>
    this.translate.instant(this.expenseId() ? 'expenses.editTitle' : 'expenses.createTitle'),
  );

  protected readonly showRecurrence = computed(() => !this.expenseId());
  protected readonly showScope = computed(() => !!this.editing()?.seriesId);
  protected readonly recurrencePreview = computed(() => {
    const value = this.formValue();
    const count = Number(value.recurrenceCount);
    if (!value.recurrenceEnabled || !value.competence || !Number.isFinite(count)) return '';
    return competenceRangePreview(value.competence, count, this.language.current());
  });

  constructor() {
    effect(() => {
      if (!this.open()) return;
      const id = this.expenseId();
      if (id) {
        this.loadExpense(id);
      } else {
        this.editing.set(null);
        this.form.reset({
          name: '',
          categoryId: this.categories()[0]?.id ?? '',
          type: ExpenseType.Variable,
          amount: null,
          competence: this.competence(),
          dueDay: '',
          isPaid: false,
          notes: '',
          recurrenceEnabled: false,
          recurrenceCount: '2',
          scope: ExpenseChangeScope.This,
        });
      }
    });

    this.form.controls.categoryId.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (value === CREATE_CATEGORY_VALUE) {
        this.form.controls.categoryId.setValue('', { emitEvent: false });
        this.categoryDialogOpen.set(true);
      }
    });
  }

  protected confirmClose = (): boolean => {
    if (!this.form.dirty) return true;
    this.discardOpen.set(true);
    return false;
  };

  protected discardConfirmed(): void {
    this.discardOpen.set(false);
    this.form.markAsPristine();
    this.closed.emit();
  }

  protected discardCancelled(): void {
    this.discardOpen.set(false);
  }

  protected cancel(): void {
    if (this.form.dirty) {
      this.discardOpen.set(true);
      return;
    }
    this.closed.emit();
  }

  protected onCategorySaved(id: string): void {
    this.categoryDialogOpen.set(false);
    this.categoriesChanged.emit();
    this.form.controls.categoryId.setValue(id);
    this.form.controls.categoryId.markAsDirty();
  }

  protected onCategoryDialogClosed(): void {
    this.categoryDialogOpen.set(false);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    if (!value.competence || !value.amount || !value.categoryId) return;
    const recurrenceCount = Number(value.recurrenceCount);
    if (value.recurrenceEnabled && (!Number.isFinite(recurrenceCount) || recurrenceCount < 2 || recurrenceCount > 60)) {
      this.form.controls.recurrenceCount.setErrors({ serverError: 'invalid_recurrence_count' });
      return;
    }

    const payload = {
      categoryId: value.categoryId,
      name: value.name.trim(),
      type: value.type,
      amount: value.amount,
      year: value.competence.year,
      month: value.competence.month,
      dueDay: value.dueDay ? Number(value.dueDay) : null,
      isPaid: value.isPaid,
      notes: value.notes.trim() || null,
    };

    this.saving.set(true);
    const id = this.expenseId();
    const onSuccess = (): void => {
      this.saving.set(false);
      this.form.markAsPristine();
      this.toastr.success(this.translate.instant(id ? 'toast.expenseUpdated' : 'toast.expenseCreated'));
      this.saved.emit();
      this.closed.emit();
    };
    const onError = (error: unknown): void => {
      this.saving.set(false);
      applyServerErrors(this.form, error);
      if (error instanceof HttpErrorResponse && error.status === 409) {
        const code = apiErrorCode(error);
        if (code === 'too_many_expenses_in_month') {
          this.toastr.warning(this.translate.instant('errors.codes.too_many_expenses_in_month'));
        }
      }
    };

    if (id) {
      this.expenses
        .update(id, payload, value.scope)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({ next: onSuccess, error: onError });
      return;
    }

    this.expenses
      .create({
        ...payload,
        recurrence: value.recurrenceEnabled ? ExpenseRecurrence.Monthly : ExpenseRecurrence.None,
        recurrenceCount: value.recurrenceEnabled ? recurrenceCount : null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: onSuccess, error: onError });
  }

  private loadExpense(id: string): void {
    this.loading.set(true);
    this.expenses
      .get(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (expense) => {
          this.editing.set(expense);
          this.form.reset({
            name: expense.name,
            categoryId: expense.categoryId,
            type: expense.type,
            amount: expense.amount,
            competence: { year: expense.year, month: expense.month },
            dueDay: expense.dueDay == null ? '' : String(expense.dueDay),
            isPaid: expense.isPaid,
            notes: expense.notes ?? '',
            recurrenceEnabled: false,
            recurrenceCount: '2',
            scope: ExpenseChangeScope.This,
          });
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.closed.emit();
        },
      });
  }
}
