import { AbstractControl, ValidationErrors } from '@angular/forms';
import { isValidPhoneNumber } from 'libphonenumber-js';
import { postalCodeFormat } from '../masks/postal-code';

const MIN_YEAR = 1900;

export function birthDateValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string | null;
  if (!value) return null;

  const date = new Date(`${value}T00:00:00`);
  if (Number.isNaN(date.getTime())) return { birthDate: true };

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return date > today || date.getFullYear() < MIN_YEAR ? { birthDate: true } : null;
}

export function phoneValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string | null;
  if (!value) return null;
  return isValidPhoneNumber(value) ? null : { phone: true };
}

export function pictureUrlValidator(control: AbstractControl): ValidationErrors | null {
  const value = (control.value as string | null)?.trim();
  if (!value) return null;
  try {
    const url = new URL(value);
    return url.protocol === 'http:' || url.protocol === 'https:' ? null : { pictureUrl: true };
  } catch {
    return { pictureUrl: true };
  }
}

export function addressValidator(group: AbstractControl): ValidationErrors | null {
  const value = group.value as Record<string, string>;
  const required = ['postalCode', 'street', 'number', 'neighborhood', 'city', 'state', 'country'];
  const anyFilled = Object.values(value).some((field) => (field ?? '').trim() !== '');
  if (!anyFilled) return null;
  return required.some((key) => (value[key] ?? '').trim() === '') ? { addressIncomplete: true } : null;
}

export function postalCodeValidator(control: AbstractControl): ValidationErrors | null {
  const value = ((control.value as string | null) ?? '').trim();
  if (!value) return null;
  const country = (control.parent?.get('country')?.value as string | null) ?? '';
  const format = country ? postalCodeFormat(country) : null;
  return format && value.length < format.minLength ? { postalCode: true } : null;
}
