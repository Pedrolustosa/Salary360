import { ChangeDetectionStrategy, Component, ElementRef, HostListener, computed, effect, forwardRef, inject, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageService } from '../../core/i18n/language.service';
import { applyMask } from '../../core/masks/apply-mask';
import { Icon } from '../icon';
import { DATE_MASK, MONTH_FIRST_LANGUAGES, SUNDAY_FIRST_LANGUAGES, YEARS_PER_PAGE } from './constants/date-formats';
import { CalendarView } from './enums/calendar-view.enum';
import { CalendarDay } from './interfaces/calendar-day.interface';

function toIso(year: number, month: number, day: number): string {
  return `${String(year).padStart(4, '0')}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

@Component({
  selector: 'app-date-picker',
  imports: [TranslatePipe, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './date-picker.html',
  styleUrl: './date-picker.scss',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DatePicker), multi: true }],
})
export class DatePicker implements ControlValueAccessor {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly language = inject(LanguageService);

  readonly inputId = input('date');
  readonly min = input('1900-01-01');
  readonly max = input('2100-12-31');

  protected readonly CalendarView = CalendarView;
  protected readonly maskLength = DATE_MASK.length;
  protected readonly text = signal('');
  protected readonly selected = signal('');
  protected readonly open = signal(false);
  protected readonly disabled = signal(false);
  protected readonly view = signal(CalendarView.Days);
  protected readonly viewYear = signal(new Date().getFullYear());
  protected readonly viewMonth = signal(new Date().getMonth());
  protected readonly yearPageStart = signal(0);

  protected readonly monthFirst = computed(() => MONTH_FIRST_LANGUAGES.includes(this.language.current()));
  protected readonly todayIso = toIso(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());

  protected readonly monthNames = computed(() => {
    const formatter = new Intl.DateTimeFormat(this.language.current(), { month: 'long' });
    return Array.from({ length: 12 }, (_, month) => formatter.format(new Date(2024, month, 1)));
  });

  protected readonly shortMonthNames = computed(() => {
    const formatter = new Intl.DateTimeFormat(this.language.current(), { month: 'short' });
    return Array.from({ length: 12 }, (_, month) => formatter.format(new Date(2024, month, 1)));
  });

  private readonly weekStart = computed(() => (SUNDAY_FIRST_LANGUAGES.includes(this.language.current()) ? 0 : 1));

  protected readonly weekdays = computed(() => {
    const formatter = new Intl.DateTimeFormat(this.language.current(), { weekday: 'short' });
    return Array.from({ length: 7 }, (_, index) => formatter.format(new Date(2024, 0, 7 + ((this.weekStart() + index) % 7))));
  });

  protected readonly days = computed<CalendarDay[]>(() => {
    const year = this.viewYear();
    const month = this.viewMonth();
    const offset = (new Date(year, month, 1).getDay() - this.weekStart() + 7) % 7;
    const start = new Date(year, month, 1 - offset);

    return Array.from({ length: 42 }, (_, index) => {
      const date = new Date(start.getFullYear(), start.getMonth(), start.getDate() + index);
      const iso = toIso(date.getFullYear(), date.getMonth(), date.getDate());
      return {
        iso,
        day: date.getDate(),
        inMonth: date.getMonth() === month,
        disabled: !this.inRange(iso),
        selected: iso === this.selected(),
        today: iso === this.todayIso,
      };
    });
  });

  protected readonly years = computed(() =>
    Array.from({ length: YEARS_PER_PAGE }, (_, index) => this.yearPageStart() + index),
  );

  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor() {
    effect(() => {
      this.language.current();
      const iso = this.selected();
      if (iso) this.text.set(this.format(iso));
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

  writeValue(value: string | null): void {
    const iso = value ?? '';
    this.selected.set(iso);
    this.text.set(iso ? this.format(iso) : '');
    if (iso) this.goTo(iso);
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
    if (isDisabled) this.open.set(false);
  }

  protected placeholderKey(): string {
    return this.monthFirst() ? 'date.placeholderMdy' : 'date.placeholderDmy';
  }

  protected onInput(event: Event): void {
    const element = event.target as HTMLInputElement;
    const masked = applyMask(element.value, DATE_MASK);
    element.value = masked;
    this.text.set(masked);

    const iso = masked.length === DATE_MASK.length ? this.parse(masked) : '';
    this.selected.set(iso);
    if (iso) this.goTo(iso);
    this.onChange(iso);
  }

  protected onBlur(): void {
    this.onTouched();
  }

  protected toggle(): void {
    if (this.disabled()) return;
    if (this.open()) {
      this.close();
      return;
    }
    if (!this.selected()) this.goTo(this.clamp(this.todayIso));
    this.view.set(CalendarView.Days);
    this.open.set(true);
  }

  protected pick(day: CalendarDay): void {
    if (day.disabled) return;
    this.selected.set(day.iso);
    this.text.set(this.format(day.iso));
    this.onChange(day.iso);
    this.close();
  }

  protected previous(): void {
    if (this.view() === CalendarView.Days) this.shiftMonth(-1);
    else if (this.view() === CalendarView.Months) this.viewYear.update((year) => year - 1);
    else this.yearPageStart.update((start) => start - YEARS_PER_PAGE);
  }

  protected next(): void {
    if (this.view() === CalendarView.Days) this.shiftMonth(1);
    else if (this.view() === CalendarView.Months) this.viewYear.update((year) => year + 1);
    else this.yearPageStart.update((start) => start + YEARS_PER_PAGE);
  }

  protected showMonths(): void {
    this.view.set(CalendarView.Months);
  }

  protected showYears(): void {
    this.yearPageStart.set(this.viewYear() - (this.viewYear() % YEARS_PER_PAGE));
    this.view.set(CalendarView.Years);
  }

  protected chooseYear(year: number): void {
    if (!this.yearAllowed(year)) return;
    this.viewYear.set(year);
    this.view.set(CalendarView.Months);
  }

  protected chooseMonth(month: number): void {
    if (!this.monthAllowed(this.viewYear(), month)) return;
    this.viewMonth.set(month);
    this.view.set(CalendarView.Days);
  }

  protected yearAllowed(year: number): boolean {
    return year >= this.yearOf(this.min()) && year <= this.yearOf(this.max());
  }

  protected monthAllowed(year: number, month: number): boolean {
    const first = toIso(year, month, 1);
    const last = toIso(year, month, new Date(year, month + 1, 0).getDate());
    return last >= this.min() && first <= this.max();
  }

  private shiftMonth(delta: number): void {
    const date = new Date(this.viewYear(), this.viewMonth() + delta, 1);
    this.viewYear.set(date.getFullYear());
    this.viewMonth.set(date.getMonth());
  }

  private goTo(iso: string): void {
    const [year, month] = iso.split('-').map(Number);
    this.viewYear.set(year);
    this.viewMonth.set(month - 1);
  }

  private yearOf(iso: string): number {
    return Number(iso.split('-')[0]);
  }

  private inRange(iso: string): boolean {
    return iso >= this.min() && iso <= this.max();
  }

  private clamp(iso: string): string {
    if (iso > this.max()) return this.max();
    if (iso < this.min()) return this.min();
    return iso;
  }

  private format(iso: string): string {
    const [year, month, day] = iso.split('-');
    return this.monthFirst() ? `${month}/${day}/${year}` : `${day}/${month}/${year}`;
  }

  private parse(text: string): string {
    const [first, second, year] = text.split('/').map(Number);
    const day = this.monthFirst() ? second : first;
    const month = this.monthFirst() ? first : second;
    const date = new Date(year, month - 1, day);
    const valid = date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;
    if (!valid || year < 1000) return '';
    const iso = toIso(year, month - 1, day);
    return this.inRange(iso) ? iso : '';
  }

  private close(): void {
    this.open.set(false);
    this.onTouched();
  }
}
