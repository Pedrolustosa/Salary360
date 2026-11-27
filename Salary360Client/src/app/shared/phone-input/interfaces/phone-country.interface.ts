import { CountryCode } from 'libphonenumber-js';

export interface PhoneCountry {
  code: CountryCode;
  name: string;
  dialCode: string;
  flagUrl: string;
}
