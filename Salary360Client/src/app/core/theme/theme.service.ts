import { Injectable, signal } from '@angular/core';
import { ThemeMode } from './enums/theme-mode.enum';

const STORAGE_KEY = 'salary360.theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly mode = signal<ThemeMode>(ThemeMode.Light);
  readonly current = this.mode.asReadonly();

  init(): void {
    this.set(this.resolveInitialMode());
  }

  toggle(): void {
    this.set(this.mode() === ThemeMode.Dark ? ThemeMode.Light : ThemeMode.Dark);
  }

  set(mode: ThemeMode): void {
    this.mode.set(mode);
    document.documentElement.classList.toggle('dark', mode === ThemeMode.Dark);
    document.documentElement.style.colorScheme = mode;
    try {
      localStorage.setItem(STORAGE_KEY, mode);
    } catch {
      return;
    }
  }

  private resolveInitialMode(): ThemeMode {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored === ThemeMode.Light || stored === ThemeMode.Dark) return stored;
    } catch {
      return ThemeMode.Light;
    }
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? ThemeMode.Dark : ThemeMode.Light;
  }
}
