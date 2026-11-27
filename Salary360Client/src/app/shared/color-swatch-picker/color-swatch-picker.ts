import { ChangeDetectionStrategy, Component, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CategoryColor } from '../../features/expenses/enums/category-color.enum';
import { CATEGORY_COLOR_CLASSES } from '../../features/expenses/utils/category-color-classes';

@Component({
  selector: 'app-color-swatch-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './color-swatch-picker.html',
  styleUrl: './color-swatch-picker.scss',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ColorSwatchPicker), multi: true }],
})
export class ColorSwatchPicker implements ControlValueAccessor {
  readonly ariaLabel = input('');

  protected readonly colors = Object.values(CategoryColor);
  protected readonly value = signal<CategoryColor>(CategoryColor.Slate);
  protected readonly disabled = signal(false);

  private onChange: (value: CategoryColor) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  writeValue(value: CategoryColor | null): void {
    this.value.set(value ?? CategoryColor.Slate);
  }

  registerOnChange(fn: (value: CategoryColor) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  protected swatchClass(color: CategoryColor): string {
    const classes = CATEGORY_COLOR_CLASSES[color];
    return this.value() === color ? `${classes.bg} ring-2 ${classes.ring}` : classes.bg;
  }

  protected select(color: CategoryColor): void {
    if (this.disabled()) return;
    this.value.set(color);
    this.onChange(color);
    this.onTouched();
  }
}
