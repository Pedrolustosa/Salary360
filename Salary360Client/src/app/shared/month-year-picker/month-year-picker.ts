import { ChangeDetectionStrategy, Component, ElementRef, HostListener, computed, effect, forwardRef, inject, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageService } from '../../core/i18n/language.service';
import { Icon } from '../icon';
import { MonthYearValue } from './interfaces/month-year-value.interface';

@Component({
  selector: 'app-month-year-picker',
  imports: [TranslatePipe, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './month-year-picker.html',
  styleUrl: './month-year-picker.scss',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MonthYearPicker), multi: true }],
})
export class MonthYearPicker implements ControlValueAccessor {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly language = inject(LanguageService);

  readonly inputId = input('competence');

  protected readonly text = signal('');
  protected readonly selected = signal<MonthYearValue | null>(null);
  protected readonly open = signal(false);
  protected readonly disabled = signal(false);
  protected readonly viewYear = signal(new Date().getFullYear());

  private readonly now = new Date();
  private readonly maxYear = this.now.getFullYear();
  private readonly maxMonth = this.now.getMonth() + 1;

  protected readonly monthNames = computed(() => {
    const formatter = new Intl.DateTimeFormat(this.language.current(), { month: 'long' });
    return Array.from({ length: 12 }, (_, month) => formatter.format(new Date(2024, month, 1)));
  });

  protected readonly shortMonthNames = computed(() => {
    const formatter = new Intl.DateTimeFormat(this.language.current(), { month: 'short' });
    return Array.from({ length: 12 }, (_, month) => formatter.format(new Date(2024, month, 1)));
  });

  private onChange: (value: MonthYearValue | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor() {
    effect(() => {
      this.language.current();
      const value = this.selected();
      this.text.set(value ? this.format(value) : '');
    });
  }

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (this.open() && !this.host.nativeElement.contains(event.target as Node)) this.close();
  }

  @HostListener('keydown.escape')
  protected onEscape(): void {
    if (this.open()) this.close();
  }

  writeValue(value: MonthYearValue | null): void {
    this.selected.set(value);
    this.text.set(value ? this.format(value) : '');
    if (value) this.viewYear.set(value.year);
  }

  registerOnChange(fn: (value: MonthYearValue | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
    if (isDisabled) this.open.set(false);
  }

  protected toggle(): void {
    if (this.disabled()) return;
    if (this.open()) {
      this.close();
      return;
    }
    const current = this.selected();
    this.viewYear.set(current?.year ?? this.maxYear);
    this.open.set(true);
  }

  protected previousYear(): void {
    this.viewYear.update((year) => Math.max(1900, year - 1));
  }

  protected nextYear(): void {
    this.viewYear.update((year) => Math.min(this.maxYear, year + 1));
  }

  protected monthAllowed(month: number): boolean {
    const year = this.viewYear();
    if (year < 1900 || year > this.maxYear) return false;
    if (year === this.maxYear && month > this.maxMonth) return false;
    return month >= 1 && month <= 12;
  }

  protected pick(month: number): void {
    if (!this.monthAllowed(month)) return;
    const value = { year: this.viewYear(), month };
    this.selected.set(value);
    this.text.set(this.format(value));
    this.onChange(value);
    this.close();
  }

  protected isSelected(month: number): boolean {
    const value = this.selected();
    return !!value && value.year === this.viewYear() && value.month === month;
  }

  private format(value: MonthYearValue): string {
    const name = this.monthNames()[value.month - 1] ?? '';
    return `${name.charAt(0).toUpperCase()}${name.slice(1)} ${value.year}`;
  }

  protected markTouched(): void {
    this.onTouched();
  }

  private close(): void {
    this.open.set(false);
    this.onTouched();
  }
}
