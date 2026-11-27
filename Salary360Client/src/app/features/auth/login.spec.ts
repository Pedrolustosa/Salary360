import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { throwError } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { Login } from './login';

describe('Login', () => {
  const auth = { login: vi.fn(), rememberedEmail: vi.fn(() => '') };
  let fixture: ComponentFixture<Login>;

  function root(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function fill(selector: string, value: string): void {
    const input = root().querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  function submit(): void {
    root().querySelector<HTMLButtonElement>('button[type=submit]')!.click();
  }

  async function settle(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
  }

  beforeEach(async () => {
    vi.useFakeTimers();
    vi.clearAllMocks();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideTranslateService({ fallbackLang: 'pt-BR' }),
        { provide: AuthService, useValue: auth },
      ],
    });
    fixture = TestBed.createComponent(Login);
    await settle();
  });

  afterEach(() => vi.useRealTimers());

  it('should not call the API while the form is invalid', async () => {
    submit();
    await settle();

    expect(auth.login).not.toHaveBeenCalled();
    expect(root().querySelectorAll('.field-error').length).toBeGreaterThan(0);
  });

  it('should show the invalid credentials message inline', async () => {
    auth.login.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401, error: { code: 'invalid_credentials' } })),
    );
    fill('#email', 'ana@example.com');
    fill('#password', 'wrong');
    submit();
    await settle();

    expect(root().textContent).toContain('errors.invalidCredentials');
    expect(root().querySelector<HTMLButtonElement>('button[type=submit]')!.disabled).toBe(false);
  });

  it('should block the button with a countdown after a 429 and release it afterwards', async () => {
    auth.login.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 429, error: { code: 'too_many_requests', retryAfterSeconds: 3 } })),
    );
    fill('#email', 'ana@example.com');
    fill('#password', 'wrong');
    submit();
    await settle();

    const button = root().querySelector<HTMLButtonElement>('button[type=submit]')!;
    expect(button.disabled).toBe(true);
    expect(root().textContent).toContain('auth.tryAgainIn');

    submit();
    expect(auth.login).toHaveBeenCalledTimes(1);

    await vi.advanceTimersByTimeAsync(3000);
    await settle();

    expect(button.disabled).toBe(false);
    expect(root().textContent).not.toContain('auth.tryAgainIn');
  });
});
