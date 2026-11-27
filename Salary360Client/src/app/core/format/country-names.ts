import { getCountries } from 'libphonenumber-js';

export interface CountryOption {
  code: string;
  name: string;
}

export function listCountries(locale: string): CountryOption[] {
  const names = new Intl.DisplayNames([locale], { type: 'region' });
  return getCountries()
    .map((code) => ({ code: code as string, name: names.of(code) ?? code }))
    .sort((a, b) => a.name.localeCompare(b.name, locale));
}
