import { DEFAULT_POSTAL_MAX_LENGTH, POSTAL_CODE_FORMATS } from './constants/postal-code-formats';
import { PostalCodeFormat } from './interfaces/postal-code-format.interface';

export function postalCodeFormat(country: string): PostalCodeFormat | null {
  return POSTAL_CODE_FORMATS[country.toUpperCase()] ?? null;
}

export function postalCodeMaxLength(country: string): number {
  return postalCodeFormat(country)?.pattern.length ?? DEFAULT_POSTAL_MAX_LENGTH;
}
