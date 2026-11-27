import { PostalCodeFormat } from '../interfaces/postal-code-format.interface';

export const DEFAULT_POSTAL_MAX_LENGTH = 12;

export const POSTAL_CODE_FORMATS: Record<string, PostalCodeFormat> = {
  BR: { pattern: '00000-000', minLength: 9, placeholder: '00000-000' },
  US: { pattern: '00000-0000', minLength: 5, placeholder: '00000' },
  PT: { pattern: '0000-000', minLength: 8, placeholder: '0000-000' },
  ES: { pattern: '00000', minLength: 5, placeholder: '00000' },
  FR: { pattern: '00000', minLength: 5, placeholder: '00000' },
  DE: { pattern: '00000', minLength: 5, placeholder: '00000' },
  IT: { pattern: '00000', minLength: 5, placeholder: '00000' },
  MX: { pattern: '00000', minLength: 5, placeholder: '00000' },
  UY: { pattern: '00000', minLength: 5, placeholder: '00000' },
  PE: { pattern: '00000', minLength: 5, placeholder: '00000' },
  CL: { pattern: '0000000', minLength: 7, placeholder: '0000000' },
  CO: { pattern: '000000', minLength: 6, placeholder: '000000' },
  PY: { pattern: '0000', minLength: 4, placeholder: '0000' },
  AU: { pattern: '0000', minLength: 4, placeholder: '0000' },
  AR: { pattern: 'A0000AAA', minLength: 8, placeholder: 'C1425ABC' },
  CA: { pattern: 'A0A 0A0', minLength: 7, placeholder: 'K1A 0B1' },
  NL: { pattern: '0000 AA', minLength: 7, placeholder: '1234 AB' },
  JP: { pattern: '000-0000', minLength: 8, placeholder: '100-0001' },
};
