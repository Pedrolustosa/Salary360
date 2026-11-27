import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
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
  protected readonly loading = signal(false);

  constructor() {
    const subscription = inject(Router).events.subscribe((event) => {
      if (event instanceof NavigationStart) this.loading.set(true);
      if (event instanceof NavigationEnd || event instanceof NavigationCancel || event instanceof NavigationError) {
        this.loading.set(false);
      }
    });
    inject(DestroyRef).onDestroy(() => subscription.unsubscribe());
  }
}
