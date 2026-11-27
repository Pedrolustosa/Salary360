import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { map } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { Icon } from '../../shared/icon';
import { Toolbar } from '../../shared/toolbar';

const KNOWN_CODES = ['403', '404', '500'];
const ICON_BY_CODE: Record<string, string> = {
  '403': 'lock-closed',
  '404': 'magnifying-glass',
  '500': 'exclamation-triangle',
};

@Component({
  selector: 'app-error-page',
  imports: [RouterLink, TranslatePipe, Icon, Toolbar],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './error-page.html',
  styleUrl: './error-page.scss',
})
export class ErrorPage {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);

  private readonly rawCode = toSignal(inject(ActivatedRoute).paramMap.pipe(map((params) => params.get('code') ?? '')), {
    initialValue: '',
  });

  protected readonly code = computed(() => (KNOWN_CODES.includes(this.rawCode()) ? this.rawCode() : 'generic'));
  protected readonly icon = computed(() => ICON_BY_CODE[this.code()] ?? 'exclamation-triangle');
  protected readonly homeLink = computed(() => (this.auth.isAuthenticated() ? '/' : '/login'));

  protected reload(): void {
    window.location.assign(this.router.url.startsWith('/error') ? this.homeLink() : this.router.url);
  }

  protected back(): void {
    if (window.history.length > 1) {
      window.history.back();
      return;
    }
    void this.router.navigateByUrl(this.homeLink());
  }
}
