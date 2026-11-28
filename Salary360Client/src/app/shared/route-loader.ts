import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Spinner } from './spinner';

@Component({
  selector: 'app-route-loader',
  imports: [TranslatePipe, Spinner],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './route-loader.html',
  styleUrl: './route-loader.scss',
})
export class RouteLoader {
  private readonly destroyRef = inject(DestroyRef);
  protected readonly loading = signal(false);

  constructor() {
    inject(Router)
      .events.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((event) => {
        if (event instanceof NavigationStart) this.loading.set(true);
        if (event instanceof NavigationEnd || event instanceof NavigationCancel || event instanceof NavigationError) {
          this.loading.set(false);
        }
      });
  }
}
