import { Injectable, computed, inject, signal } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { todayIsoDate } from '../format/date-format';

const STORAGE_PREFIX = 'salary360.birthday.';

@Injectable({ providedIn: 'root' })
export class BirthdayService {
  private readonly auth = inject(AuthService);
  private readonly celebratingState = signal(false);

  readonly celebrating = this.celebratingState.asReadonly();

  readonly isBirthday = computed(() => this.matchesToday(this.auth.session()?.dateOfBirth ?? null));

  celebrateOncePerDay(): void {
    const userId = this.auth.session()?.userId;
    if (!userId || !this.isBirthday()) return;

    const key = `${STORAGE_PREFIX}${userId}`;
    let alreadyCelebrated = false;
    try {
      alreadyCelebrated = localStorage.getItem(key) === todayIsoDate();
      if (!alreadyCelebrated) localStorage.setItem(key, todayIsoDate());
    } catch {
      alreadyCelebrated = false;
    }

    if (!alreadyCelebrated) this.celebratingState.set(true);
  }

  celebrate(): void {
    if (this.isBirthday()) this.celebratingState.set(true);
  }

  dismiss(): void {
    this.celebratingState.set(false);
  }

  private matchesToday(dateOfBirth: string | null): boolean {
    if (!dateOfBirth) return false;

    const [, month, day] = dateOfBirth.split('-').map(Number);
    const today = new Date();
    const isLeapYear = new Date(today.getFullYear(), 1, 29).getMonth() === 1;
    const effectiveDay = month === 2 && day === 29 && !isLeapYear ? 28 : day;

    return today.getMonth() + 1 === month && today.getDate() === effectiveDay;
  }
}
