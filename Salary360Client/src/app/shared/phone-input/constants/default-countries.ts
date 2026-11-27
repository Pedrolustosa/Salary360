import { CountryCode } from 'libphonenumber-js';
import { LanguageCode } from '../../../core/i18n/enums/language-code.enum';

export const DEFAULT_COUNTRY_BY_LANGUAGE: Record<LanguageCode, CountryCode> = {
  [LanguageCode.PtBr]: 'BR',
  [LanguageCode.EnUs]: 'US',
  [LanguageCode.EsEs]: 'ES',
};
