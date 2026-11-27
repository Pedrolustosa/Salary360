import { ChangeDetectionStrategy, Component, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { SegmentedOption } from './interfaces/segmented-option.interface';

@Component({
  selector: 'app-segmented-control',
  imports: [TranslatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './segmented-control.html',
  styleUrl: './segmented-control.scss',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SegmentedControl), multi: true }],
})
export class SegmentedControl implements ControlValueAccessor {
  readonly options = input.required<SegmentedOption[]>();
  readonly ariaLabel = input('');

  protected readonly value = signal('');
  protected readonly disabled = signal(false);

  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  protected select(option: SegmentedOption): void {
    if (this.disabled()) return;
    this.value.set(option.value);
    this.onChange(option.value);
    this.onTouched();
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (this.disabled()) return;
    const options = this.options();
    if (options.length === 0) return;
    const index = options.findIndex((option) => option.value === this.value());
    let next = index;
    if (event.key === 'ArrowRight' || event.key === 'ArrowDown') {
      event.preventDefault();
      next = index < 0 ? 0 : (index + 1) % options.length;
    } else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') {
      event.preventDefault();
      next = index < 0 ? options.length - 1 : (index - 1 + options.length) % options.length;
    } else if (event.key === 'Home') {
      event.preventDefault();
      next = 0;
    } else if (event.key === 'End') {
      event.preventDefault();
      next = options.length - 1;
    } else {
      return;
    }
    this.select(options[next]);
  }
}
