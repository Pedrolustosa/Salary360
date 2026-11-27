import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BirthdayService } from '../core/birthday/birthday.service';
import { NotificationService } from '../core/notifications/notification.service';
import { BirthdayCelebration } from './birthday-celebration';
import { Navbar } from './navbar';
import { Sidebar } from './sidebar';

const STORAGE_KEY = 'salary360.sidebar.collapsed';
const DESKTOP_QUERY = '(min-width: 1024px)';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, Sidebar, Navbar, BirthdayCelebration],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell implements OnInit, OnDestroy {
  private readonly notifications = inject(NotificationService);

  protected readonly birthday = inject(BirthdayService);

  protected readonly sidebarOpen = signal(false);
  protected readonly sidebarCollapsed = signal(this.restoreCollapsed());

  ngOnInit(): void {
    void this.notifications.connect();
    this.birthday.celebrateOncePerDay();
  }

  ngOnDestroy(): void {
    void this.notifications.disconnect();
  }

  protected toggleSidebar(): void {
    if (window.matchMedia(DESKTOP_QUERY).matches) {
      this.toggleCollapsed();
      return;
    }
    this.sidebarOpen.update((value) => !value);
  }

  protected toggleCollapsed(): void {
    const next = !this.sidebarCollapsed();
    this.sidebarCollapsed.set(next);
    try {
      localStorage.setItem(STORAGE_KEY, String(next));
    } catch {
      return;
    }
  }

  protected closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  private restoreCollapsed(): boolean {
    try {
      return localStorage.getItem(STORAGE_KEY) === 'true';
    } catch {
      return false;
    }
  }
}
