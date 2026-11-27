import { ChangeDetectionStrategy, Component, ElementRef, HostListener, computed, forwardRef, inject, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import {
  AsYouType,
  CountryCode,
  getCountries,
  getCountryCallingCode,
  getExampleNumber,
  parsePhoneNumberFromString,
  validatePhoneNumberLength,
} from 'libphonenumber-js';
import examples from 'libphonenumber-js/mobile/examples';
import { LanguageService } from '../../core/i18n/language.service';
import { Icon } from '../icon';
import { DEFAULT_COUNTRY_BY_LANGUAGE } from './constants/default-countries';
import { PhoneCountry } from './interfaces/phone-country.interface';

@Component({
  selector: 'app-phone-input',
  imports: [TranslatePipe, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './phone-input.html',
  styleUrl: './phone-input.scss',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => PhoneInput), multi: true }],
})
export class PhoneInput implements ControlValueAccessor {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly language = inject(LanguageService);

  readonly inputId = input('phoneNumber');

  protected readonly country = signal<CountryCode>(DEFAULT_COUNTRY_BY_LANGUAGE[this.language.current()] ?? 'US');
  protected readonly national = signal('');
  protected readonly open = signal(false);
  protected readonly query = signal('');
  protected readonly disabled = signal(false);

  protected readonly countries = computed<PhoneCountry[]>(() => {
    const locale = this.language.current();
    const names = new Intl.DisplayNames([locale], { type: 'region' });
    return getCountries()
      .map((code) => ({
        code,
        name: names.of(code) ?? code,
        dialCode: `+${getCountryCallingCode(code)}`,
        flagUrl: `url(flags/${code.toLowerCase()}.svg)`,
      }))
      .sort((a, b) => a.name.localeCompare(b.name, locale));
  });

  protected readonly filtered = computed(() => {
    const term = this.query().trim().toLowerCase();
    if (!term) return this.countries();
    return this.countries().filter(
      (country) =>
        country.name.toLowerCase().includes(term) ||
        country.dialCode.includes(term) ||
        country.code.toLowerCase() === term,
    );
  });

  protected readonly selected = computed(() => this.countries().find((country) => country.code === this.country()));
  protected readonly placeholder = computed(() => getExampleNumber(this.country(), examples)?.formatNational() ?? '');

  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (this.open() && !this.host.nativeElement.contains(event.target as Node)) this.close();
  }

  writeValue(value: string | null): void {
    if (!value) {
      this.national.set('');
      return;
    }

    const parsed = parsePhoneNumberFromString(value);
    if (parsed) {
      if (parsed.country) this.country.set(parsed.country);
      this.national.set(parsed.formatNational());
      return;
    }

    this.national.set(value);
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
    if (isDisabled) this.close();
  }

  protected toggle(): void {
    if (this.disabled()) return;
    this.open.update((value) => !value);
    this.query.set('');
  }

  protected onInput(event: Event): void {
    const element = event.target as HTMLInputElement;
    const raw = element.value;
    let country = this.country();
    let digits = raw.replace(/\D/g, '');

    if (raw.trim().startsWith('+')) {
      const probe = new AsYouType();
      probe.input(raw);
      const detected = probe.getNumber();
      if (detected?.country) {
        country = detected.country;
        digits = detected.nationalNumber;
        this.country.set(country);
      }
    }

    const { formatted, value } = this.limit(country, digits);
    element.value = formatted;
    this.national.set(formatted);
    this.onChange(value);
  }

  protected onBlur(): void {
    this.onTouched();
  }

  protected select(country: PhoneCountry): void {
    this.country.set(country.code);
    const { formatted, value } = this.limit(country.code, this.national().replace(/\D/g, ''));
    this.national.set(formatted);
    this.onChange(value);
    this.close();
  }

  protected onSearch(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }

  private limit(country: CountryCode, rawDigits: string): { formatted: string; value: string } {
    let digits = rawDigits;

    for (;;) {
      const formatter = new AsYouType(country);
      const formatted = formatter.input(digits);
      const number = formatter.getNumber();
      const tooLong = number !== undefined && validatePhoneNumberLength(number.number) === 'TOO_LONG';

      if (!tooLong || digits.length === 0) {
        const value = digits === '' ? '' : (number?.number ?? `+${getCountryCallingCode(country)}${digits}`);
        return { formatted, value };
      }

      digits = digits.slice(0, -1);
    }
  }

  private close(): void {
    this.open.set(false);
    this.query.set('');
    this.onTouched();
  }
}
