import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { applyServerErrors } from '../../../core/forms/server-errors';
import { ColorSwatchPicker } from '../../../shared/color-swatch-picker/color-swatch-picker';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { FieldError } from '../../../shared/field-error/field-error';
import { Icon } from '../../../shared/icon';
import { Modal } from '../../../shared/modal/modal';
import { CategoryColor } from '../enums/category-color.enum';
import { CategoryIcon } from '../enums/category-icon.enum';
import { ExpenseCategoryListItem } from '../interfaces/expense-category-list-item.interface';
import { ExpenseCategoryService } from '../services/expense-category.service';
import { CATEGORY_ICON_OPTIONS, categoryIconName } from '../utils/category-icon-name';

const MAX_NAME = 60;

@Component({
  selector: 'app-category-form-dialog',
  imports: [ReactiveFormsModule, TranslatePipe, Modal, ColorSwatchPicker, FieldError, Icon, ConfirmDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './category-form-dialog.html',
  styleUrl: './category-form-dialog.scss',
})
export class CategoryFormDialog {
  private readonly fb = inject(FormBuilder);
  private readonly categories = inject(ExpenseCategoryService);
  private readonly translate = inject(TranslateService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);

  readonly open = input(false);
  readonly category = input<ExpenseCategoryListItem | null>(null);

  readonly closed = output<void>();
  readonly saved = output<string>();

  protected readonly saving = signal(false);
  protected readonly discardOpen = signal(false);
  protected readonly icons = CATEGORY_ICON_OPTIONS;
  protected readonly MaxName = MAX_NAME;
  protected readonly iconName = categoryIconName;

  protected readonly form = this.fb.group({
    name: this.fb.control('', { nonNullable: true, validators: [Validators.maxLength(MAX_NAME)] }),
    colorKey: this.fb.control<CategoryColor>(CategoryColor.Slate, { nonNullable: true, validators: [Validators.required] }),
    iconKey: this.fb.control<CategoryIcon>(CategoryIcon.EllipsisHorizontal, {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  protected readonly title = computed(() =>
    this.translate.instant(this.category() ? 'expenseCategories.editTitle' : 'expenseCategories.createTitle'),
  );

  protected readonly isSystem = computed(() => !!this.category()?.systemKey);

  constructor() {
    effect(() => {
      if (!this.open()) return;
      const item = this.category();
      if (item) {
        this.form.controls.name.setValidators([Validators.maxLength(MAX_NAME)]);
        this.form.reset({
          name: item.name ?? '',
          colorKey: item.colorKey,
          iconKey: item.iconKey,
        });
      } else {
        this.form.controls.name.setValidators([Validators.required, Validators.maxLength(MAX_NAME)]);
        this.form.reset({
          name: '',
          colorKey: CategoryColor.Slate,
          iconKey: CategoryIcon.EllipsisHorizontal,
        });
      }
    });
  }

  protected selectIcon(icon: CategoryIcon): void {
    this.form.controls.iconKey.setValue(icon);
    this.form.controls.iconKey.markAsDirty();
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

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const item = this.category();
    this.saving.set(true);
    const request$ = item
      ? this.categories.update(item.id, {
          name: value.name.trim() || null,
          colorKey: value.colorKey,
          iconKey: value.iconKey,
        })
      : this.categories.create({
          name: value.name.trim(),
          colorKey: value.colorKey,
          iconKey: value.iconKey,
        });

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.saving.set(false);
        this.form.markAsPristine();
        this.toastr.success(this.translate.instant(item ? 'toast.categoryUpdated' : 'toast.categoryCreated'));
        this.saved.emit(result.id);
        this.closed.emit();
      },
      error: (error: unknown) => {
        this.saving.set(false);
        applyServerErrors(this.form, error);
      },
    });
  }
}
