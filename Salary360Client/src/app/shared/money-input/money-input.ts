import { ChangeDetectionStrategy, Component, effect, forwardRef, inject, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { formatMoney, parseMoneyDigits } from '../../core/format/money';
import { LanguageService } from '../../core/i18n/language.service';

@Component({
  selector: 'app-money-input',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './money-input.html',
  styleUrl: './money-input.scss',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MoneyInput), multi: true }],
})
export class MoneyInput implements ControlValueAccessor {
  private readonly language = inject(LanguageService);

  readonly inputId = input('money');
  readonly placeholder = input('');

  protected readonly display = signal('');
  protected readonly disabled = signal(false);

  private value: number | null = null;
  private onChange: (value: number | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor() {
    effect(() => {
      this.language.current();
      if (this.value !== null) this.display.set(formatMoney(this.value, this.language.current()));
    });
  }

  writeValue(value: number | null): void {
    this.value = value;
    this.display.set(value === null || value === undefined ? '' : formatMoney(value, this.language.current()));
  }

  registerOnChange(fn: (value: number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  protected onInput(event: Event): void {
    const element = event.target as HTMLInputElement;
    const parsed = parseMoneyDigits(element.value);
    this.value = parsed;
    this.display.set(parsed === null ? '' : formatMoney(parsed, this.language.current()));
    element.value = this.display();
    this.onChange(parsed);
  }

  protected onBlur(): void {
    this.onTouched();
  }
}
