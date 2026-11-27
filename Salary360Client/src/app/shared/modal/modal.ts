import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { Icon } from '../icon';

@Component({
  selector: 'app-modal',
  imports: [TranslatePipe, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './modal.html',
  styleUrl: './modal.scss',
})
export class Modal {
  readonly open = input(false);
  readonly title = input('');
  readonly dirty = input(false);
  readonly confirmClose = input<(() => boolean) | null>(null);
  readonly maxWidthClass = input('max-w-lg');

  readonly cancelled = output<void>();

  private readonly panel = viewChild<ElementRef<HTMLElement>>('panel');
  private readonly previouslyFocused = signal<HTMLElement | null>(null);

  constructor() {
    effect(() => {
      if (!this.open()) return;
      this.previouslyFocused.set(document.activeElement as HTMLElement | null);
      queueMicrotask(() => {
        const root = this.panel()?.nativeElement;
        if (!root) return;
        const focusable = this.focusable(root);
        (focusable[0] ?? root).focus();
      });
    });
  }

  @HostListener('document:keydown', ['$event'])
  protected onKeydown(event: KeyboardEvent): void {
    if (!this.open()) return;
    if (event.key === 'Escape') {
      event.preventDefault();
      this.requestClose();
      return;
    }
    if (event.key !== 'Tab') return;
    const root = this.panel()?.nativeElement;
    if (!root) return;
    const focusable = this.focusable(root);
    if (focusable.length === 0) {
      event.preventDefault();
      root.focus();
      return;
    }
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    const active = document.activeElement as HTMLElement | null;
    if (event.shiftKey && active === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && active === last) {
      event.preventDefault();
      first.focus();
    }
  }

  protected requestClose(): void {
    if (this.dirty()) {
      const confirm = this.confirmClose();
      if (!confirm || !confirm()) return;
    }
    this.previouslyFocused()?.focus?.();
    this.cancelled.emit();
  }

  private focusable(root: HTMLElement): HTMLElement[] {
    return Array.from(
      root.querySelectorAll<HTMLElement>(
        'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])',
      ),
    ).filter((element) => !element.hasAttribute('disabled') && element.tabIndex !== -1);
  }
}
