import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { UserStatus } from '../../core/auth/enums/user-status.enum';
import { UserProfile } from '../../core/auth/interfaces/user-profile.interface';
import { ProfilePage } from './profile-page';

const profile: UserProfile = {
  userId: 'u1',
  email: 'ana@example.com',
  displayName: 'Ana',
  dateOfBirth: '1990-03-15',
  phoneNumber: null,
  status: UserStatus.Active,
  termsAcceptedAt: null,
  privacyPolicyAcceptedAt: null,
  profilePictureUrl: null,
  address: null,
  createdAtUtc: '2026-01-01T10:00:00Z',
  updatedAtUtc: null,
  roles: ['User'],
};

describe('ProfilePage', () => {
  const auth = { getProfile: vi.fn(), updateProfile: vi.fn(), hasPermission: vi.fn() };
  let fixture: ComponentFixture<ProfilePage>;

  function root(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function tabs(): HTMLButtonElement[] {
    return Array.from(root().querySelectorAll<HTMLButtonElement>('[role=tab]'));
  }

  async function settle(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
  }

  beforeEach(async () => {
    vi.clearAllMocks();
    auth.getProfile.mockReturnValue(of(profile));
    auth.hasPermission.mockReturnValue(true);
    TestBed.configureTestingModule({
      providers: [
        provideTranslateService({ fallbackLang: 'pt-BR' }),
        { provide: AuthService, useValue: auth },
        { provide: ToastrService, useValue: { success: vi.fn() } },
      ],
    });
    fixture = TestBed.createComponent(ProfilePage);
    await settle();
  });

  it('should open on the personal tab with the e-mail disabled', () => {
    expect(tabs()).toHaveLength(3);
    expect(root().querySelector('#displayName')).not.toBeNull();
    expect(root().querySelector('#street')).toBeNull();
    expect(root().querySelector<HTMLInputElement>('#email')!.disabled).toBe(true);
  });

  it('should switch to the address tab', async () => {
    tabs()[1].click();
    await settle();

    expect(root().querySelector('#street')).not.toBeNull();
    expect(root().querySelector('#displayName')).toBeNull();
  });

  it('should show only disabled fields and no save button on the account tab', async () => {
    tabs()[2].click();
    await settle();

    const inputs = Array.from(root().querySelectorAll<HTMLInputElement>('input'));
    expect(inputs.length).toBeGreaterThan(0);
    expect(inputs.every((input) => input.disabled)).toBe(true);
    expect(root().querySelector('button[type=submit]')).toBeNull();
  });

  it('should jump to the address tab when only the address is invalid on submit', async () => {
    tabs()[1].click();
    await settle();
    const city = root().querySelector<HTMLInputElement>('#city')!;
    city.value = 'Olinda';
    city.dispatchEvent(new Event('input'));
    tabs()[0].click();
    await settle();

    root().querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await settle();

    expect(auth.updateProfile).not.toHaveBeenCalled();
    expect(root().querySelector('#street')).not.toBeNull();
  });

  it('should send a null address when it is left empty', async () => {
    auth.updateProfile.mockReturnValue(of(profile));

    root().querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await settle();

    expect(auth.updateProfile).toHaveBeenCalledWith(expect.objectContaining({ address: null, displayName: 'Ana' }));
  });
});
