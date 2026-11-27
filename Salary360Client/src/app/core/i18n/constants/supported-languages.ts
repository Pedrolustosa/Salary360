import { LanguageCode } from '../enums/language-code.enum';

export const SUPPORTED_LANGUAGES = [
  { code: LanguageCode.PtBr, label: 'Português' },
  { code: LanguageCode.EnUs, label: 'English' },
  { code: LanguageCode.EsEs, label: 'Español' },
] as const;
