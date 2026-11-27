import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { SUPPORTED_LANGUAGES } from './constants/supported-languages';
import { LanguageCode } from './enums/language-code.enum';

const DEFAULT_LANGUAGE: LanguageCode = LanguageCode.PtBr;
const STORAGE_KEY = 'salary360.language';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly translate = inject(TranslateService);

  readonly languages = SUPPORTED_LANGUAGES;
  private readonly currentLanguage = signal<LanguageCode>(DEFAULT_LANGUAGE);
  readonly current = this.currentLanguage.asReadonly();

  init(): Promise<unknown> {
    return this.set(this.resolveInitialLanguage());
  }

  set(code: LanguageCode): Promise<unknown> {
    const loaded = firstValueFrom(this.translate.use(code));
    this.currentLanguage.set(code);
    document.documentElement.lang = code;
    try {
      localStorage.setItem(STORAGE_KEY, code);
    } catch {
      return loaded;
    }
    return loaded;
  }

  private resolveInitialLanguage(): LanguageCode {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (this.isSupported(stored)) return stored;
    } catch {
      return DEFAULT_LANGUAGE;
    }

    const browser = navigator.language;
    const exact = SUPPORTED_LANGUAGES.find((l) => l.code === browser);
    const sameFamily = SUPPORTED_LANGUAGES.find((l) => l.code.startsWith(browser.split('-')[0]));
    return (exact ?? sameFamily)?.code ?? DEFAULT_LANGUAGE;
  }

  private isSupported(code: string | null): code is LanguageCode {
    return SUPPORTED_LANGUAGES.some((l) => l.code === code);
  }
}
