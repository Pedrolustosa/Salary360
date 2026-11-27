import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  inject,
  signal,
  viewChildren,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { startWith } from 'rxjs';
import { CanDeactivateComponent } from '../../../core/forms/interfaces/can-deactivate-component.interface';
import { applyServerErrors } from '../../../core/forms/server-errors';
import { formatMoney } from '../../../core/format/money';
import { LanguageService } from '../../../core/i18n/language.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { FieldError } from '../../../shared/field-error/field-error';
import { Icon } from '../../../shared/icon';
import { MoneyInput } from '../../../shared/money-input/money-input';
import { MonthYearPicker } from '../../../shared/month-year-picker/month-year-picker';
import { MonthYearValue } from '../../../shared/month-year-picker/interfaces/month-year-value.interface';
import { Select } from '../../../shared/select/select';
import { SelectOption } from '../../../shared/select/interfaces/select-option.interface';
import { Spinner } from '../../../shared/spinner';
import { SalaryItemType } from '../enums/salary-item-type.enum';
import { SalaryItemInput } from '../interfaces/salary-item-input.interface';
import { SalaryRecord } from '../interfaces/salary-record.interface';
import { SalaryService } from '../services/salary.service';
import { calculateSalarySummary } from '../utils/salary-summary';

const MAX_ITEMS = 50;
const MAX_NAME = 100;
const MAX_NOTES = 500;

@Component({
  selector: 'app-salary-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    TranslatePipe,
    FieldError,
    Icon,
    MoneyInput,
    MonthYearPicker,
    Select,
    Spinner,
    ConfirmDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './salary-form.html',
  styleUrl: './salary-form.scss',
})
export class SalaryForm implements CanDeactivateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly salaries = inject(SalaryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  private readonly toastr = inject(ToastrService);
  private readonly language = inject(LanguageService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly nameInputs = viewChildren<ElementRef<HTMLInputElement>>('itemName');

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly editId = signal<string | null>(null);
  protected readonly conflictOpen = signal(false);
  protected readonly conflictId = signal<string | null>(null);
  protected readonly MaxItems = MAX_ITEMS;
  protected readonly MaxName = MAX_NAME;
  protected readonly MaxNotes = MAX_NOTES;

  protected readonly form = this.fb.group({
    competence: this.fb.control<MonthYearValue | null>(null, { validators: [Validators.required] }),
    grossAmount: this.fb.control<number | null>(null, { validators: [Validators.required, Validators.min(0.01)] }),
    notes: this.fb.control('', { nonNullable: true, validators: [Validators.maxLength(MAX_NOTES)] }),
    items: this.fb.array([this.createItemGroup()]),
  });

  private readonly formValue = toSignal(this.form.valueChanges.pipe(startWith(this.form.getRawValue())), {
    initialValue: this.form.getRawValue(),
  });

  protected readonly summary = computed(() => {
    const value = this.formValue();
    const items = (value.items ?? []).map((item) => ({
      type: item.type ?? SalaryItemType.Benefit,
      amount: item.amount ?? null,
    }));
    return calculateSalarySummary(value.grossAmount ?? null, items);
  });

  protected readonly typeOptions = computed<SelectOption[]>(() => [
    {
      value: SalaryItemType.Benefit,
      label: this.translate.instant('salaries.itemType.Benefit'),
      textClass: 'text-emerald-600 dark:text-emerald-400 font-medium',
    },
    {
      value: SalaryItemType.Deduction,
      label: this.translate.instant('salaries.itemType.Deduction'),
      textClass: 'text-rose-600 dark:text-rose-400 font-medium',
    },
  ]);

  protected get items(): FormArray {
    return this.form.controls.items;
  }

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      const now = new Date();
      this.form.controls.competence.setValue({ year: now.getFullYear(), month: now.getMonth() + 1 });
      this.loading.set(false);
      return;
    }

    this.editId.set(id);
    this.salaries
      .get(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (record) => {
          this.apply(record);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          void this.router.navigate(['/salaries']);
        },
      });
  }

  canDeactivate(): boolean {
    if (!this.form.dirty || this.saving()) return true;
    return window.confirm(this.translate.instant('salaries.unsavedChanges'));
  }

  protected money(value: number): string {
    return formatMoney(value, this.language.current());
  }

  protected itemGroup(index: number) {
    return this.items.at(index);
  }

  protected addItem(): void {
    if (this.items.length >= MAX_ITEMS) return;
    this.items.push(this.createItemGroup());
    queueMicrotask(() => {
      const inputs = this.nameInputs();
      inputs[inputs.length - 1]?.nativeElement.focus();
    });
  }

  protected removeItem(index: number): void {
    if (this.items.length <= 1) {
      this.items.at(0).reset({ name: '', type: SalaryItemType.Benefit, amount: null });
      return;
    }
    this.items.removeAt(index);
  }

  protected cancel(): void {
    void this.router.navigate(['/salaries']);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.focusFirstInvalid();
      return;
    }

    const raw = this.form.getRawValue();
    const competence = raw.competence!;
    const payload = {
      year: competence.year,
      month: competence.month,
      grossAmount: raw.grossAmount!,
      notes: raw.notes.trim() === '' ? null : raw.notes.trim(),
      items: raw.items
        .filter((item) => (item.name ?? '').trim() !== '' || item.amount)
        .map(
          (item): SalaryItemInput => ({
            name: (item.name ?? '').trim(),
            type: (item.type as SalaryItemType) || SalaryItemType.Benefit,
            amount: item.amount ?? 0,
          }),
        ),
    };

    this.saving.set(true);
    const id = this.editId();
    const request$ = id ? this.salaries.update(id, payload) : this.salaries.create(payload);

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.form.markAsPristine();
        this.saving.set(false);
        this.toastr.success(this.translate.instant(id ? 'toast.salaryUpdated' : 'toast.salaryCreated'));
        void this.router.navigate(['/salaries']);
      },
      error: (error: unknown) => {
        this.saving.set(false);
        if (error instanceof HttpErrorResponse && error.status === 409) {
          this.handleConflict(error, competence.year, competence.month);
          return;
        }
        applyServerErrors(this.form, error);
      },
    });
  }

  protected openConflict(): void {
    const id = this.conflictId();
    this.conflictOpen.set(false);
    if (id) {
      this.form.markAsPristine();
      void this.router.navigate(['/salaries', id]);
    }
  }

  protected dismissConflict(): void {
    this.conflictOpen.set(false);
    this.conflictId.set(null);
  }

  private handleConflict(error: HttpErrorResponse, year: number, month: number): void {
    const location = error.headers?.get('Location') ?? error.headers?.get('location');
    const fromLocation = location?.match(/\/salaries\/([^/?#]+)/i)?.[1] ?? null;
    if (fromLocation) {
      this.conflictId.set(fromLocation);
      this.conflictOpen.set(true);
      return;
    }

    this.salaries
      .list(year, 1, 50)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (page) => {
          const existing = page.items.find((item) => item.month === month);
          this.conflictId.set(existing?.id ?? null);
          this.conflictOpen.set(true);
        },
        error: () => this.conflictOpen.set(true),
      });
  }

  private createItemGroup() {
    return this.fb.group({
      name: this.fb.control('', { nonNullable: true, validators: [Validators.maxLength(MAX_NAME)] }),
      type: this.fb.control(SalaryItemType.Benefit, { nonNullable: true, validators: [Validators.required] }),
      amount: this.fb.control<number | null>(null),
    });
  }

  private apply(record: SalaryRecord): void {
    while (this.items.length) this.items.removeAt(0);
    const rows = record.items.length > 0 ? record.items : [{ name: '', type: SalaryItemType.Benefit, amount: null as number | null }];
    for (const item of rows) {
      this.items.push(
        this.fb.group({
          name: this.fb.control(item.name, { nonNullable: true, validators: [Validators.maxLength(MAX_NAME)] }),
          type: this.fb.control(item.type, { nonNullable: true, validators: [Validators.required] }),
          amount: this.fb.control<number | null>(item.amount),
        }),
      );
    }
    this.form.reset({
      competence: { year: record.year, month: record.month },
      grossAmount: record.grossAmount,
      notes: record.notes ?? '',
      items: this.items.getRawValue(),
    });
  }

  private focusFirstInvalid(): void {
    const invalid = document.querySelector<HTMLElement>('.ng-invalid');
    invalid?.focus();
  }
}
