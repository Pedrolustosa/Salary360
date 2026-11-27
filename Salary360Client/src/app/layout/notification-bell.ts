import { ChangeDetectionStrategy, Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../core/i18n/language.service';
import { AppNotification } from '../core/notifications/interfaces/app-notification.interface';
import { NotificationService } from '../core/notifications/notification.service';
import { Icon } from '../shared/icon';

@Component({
  selector: 'app-notification-bell',
  imports: [TranslatePipe, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.scss',
})
export class NotificationBell {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly translate = inject(TranslateService);
  private readonly language = inject(LanguageService);

  protected readonly notifications = inject(NotificationService);
  protected readonly open = signal(false);

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (this.open() && !this.host.nativeElement.contains(event.target as Node)) this.open.set(false);
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.open.set(false);
  }

  protected toggle(): void {
    this.open.update((value) => !value);
  }

  protected title(item: AppNotification): string {
    return this.text(item, 'title', item.title);
  }

  protected message(item: AppNotification): string {
    return this.text(item, 'message', item.message);
  }

  protected time(item: AppNotification): string {
    return new Date(item.createdAtUtc).toLocaleString(this.language.current(), {
      dateStyle: 'short',
      timeStyle: 'short',
    });
  }

  private text(item: AppNotification, part: 'title' | 'message', fallback: string): string {
    this.language.current();
    const key = `notifications.types.${item.type}.${part}`;
    const translated = this.translate.instant(key);
    return translated === key ? fallback : translated;
  }
}
