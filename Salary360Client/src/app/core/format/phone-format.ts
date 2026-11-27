import { parsePhoneNumberFromString } from 'libphonenumber-js';

export function formatPhone(value: string | null | undefined): string {
  if (!value) return '';
  return parsePhoneNumberFromString(value)?.formatInternational() ?? value;
}
