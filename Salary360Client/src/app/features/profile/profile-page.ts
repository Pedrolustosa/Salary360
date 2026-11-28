import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../core/auth/auth.service';
import { applyServerErrors } from '../../core/forms/server-errors';
import { Permission } from '../../core/auth/enums/permission.enum';
import { UserAddress } from '../../core/auth/interfaces/user-address.interface';
import { UserProfile } from '../../core/auth/interfaces/user-profile.interface';
import { listCountries } from '../../core/format/country-names';
import { formatDateTime, todayIsoDate } from '../../core/format/date-format';
import { LanguageService } from '../../core/i18n/language.service';
import { applyMask } from '../../core/masks/apply-mask';
import { postalCodeFormat, postalCodeMaxLength } from '../../core/masks/postal-code';
import {
  addressValidator,
  birthDateValidator,
  phoneValidator,
  pictureUrlValidator,
  postalCodeValidator,
} from '../../core/validators/profile.validators';
import { DatePicker } from '../../shared/date-picker/date-picker';
import { PhoneInput } from '../../shared/phone-input/phone-input';
import { Select } from '../../shared/select/select';
import { SelectOption } from '../../shared/select/interfaces/select-option.interface';
import { FieldError } from '../../shared/field-error/field-error';
import { Spinner } from '../../shared/spinner';
import { ProfileTab } from './enums/profile-tab.enum';

@Component({
  selector: 'app-profile-page',
  imports: [ReactiveFormsModule, TranslatePipe, Spinner, PhoneInput, DatePicker, Select, FieldError],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.scss',
})
export class ProfilePage {
  private readonly auth = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly language = inject(LanguageService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly ProfileTab = ProfileTab;
  protected readonly tabs = [ProfileTab.Personal, ProfileTab.Address, ProfileTab.Account];
  protected readonly activeTab = signal(ProfileTab.Personal);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly profile = signal<UserProfile | null>(null);
  protected readonly pictureFailed = signal(false);
  protected readonly today = todayIsoDate();
  protected readonly canUpdate = computed(() => this.auth.hasPermission(Permission.ProfileUpdate));
  protected readonly countryOptions = computed<SelectOption[]>(() =>
    listCountries(this.language.current()).map((country) => ({
      value: country.code,
      label: country.name,
      flagUrl: `url(flags/${country.code.toLowerCase()}.svg)`,
    })),
  );
  protected readonly selectedCountry = signal('');
  protected readonly postalPlaceholder = computed(
    () => postalCodeFormat(this.selectedCountry())?.placeholder ?? this.translate.instant('placeholders.postalCode'),
  );
  protected readonly postalMaxLength = computed(() => postalCodeMaxLength(this.selectedCountry()));

  private readonly fb = inject(FormBuilder).nonNullable;

  protected readonly form = this.fb.group({
    displayName: ['', [Validators.required, Validators.maxLength(100)]],
    dateOfBirth: ['', [Validators.required, birthDateValidator]],
    phoneNumber: ['', [phoneValidator]],
    profilePictureUrl: ['', [pictureUrlValidator, Validators.maxLength(2048)]],
    address: this.fb.group(
      {
        postalCode: ['', [Validators.maxLength(20), postalCodeValidator]],
        street: ['', [Validators.maxLength(200)]],
        number: ['', [Validators.maxLength(10)]],
        complement: ['', [Validators.maxLength(100)]],
        neighborhood: ['', [Validators.maxLength(100)]],
        city: ['', [Validators.maxLength(100)]],
        state: ['', [Validators.maxLength(100)]],
        country: [''],
      },
      { validators: [addressValidator] },
    ),
  });

  protected readonly picturePreview = signal('');

  constructor() {
    this.form.controls.address.controls.country.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((country) => {
        this.selectedCountry.set(country);
        const postal = this.form.controls.address.controls.postalCode;
        const format = postalCodeFormat(country);
        postal.setValue(format ? applyMask(postal.value, format.pattern) : postal.value, { emitEvent: false });
        postal.updateValueAndValidity({ emitEvent: false });
      });

    this.form.controls.profilePictureUrl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      this.pictureFailed.set(false);
      this.picturePreview.set(this.form.controls.profilePictureUrl.valid ? value.trim() : '');
    });

    this.auth
      .getProfile()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (profile) => {
          this.apply(profile);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  protected created(): string {
    return formatDateTime(this.profile()?.createdAtUtc, this.language.current());
  }

  protected updated(): string {
    return formatDateTime(this.profile()?.updatedAtUtc, this.language.current());
  }

  protected formatted(value: string | null | undefined): string {
    return formatDateTime(value, this.language.current()) || '-';
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.activeTab.set(this.firstTabWithErrors());
      return;
    }

    this.saving.set(true);
    const { displayName, dateOfBirth, phoneNumber, profilePictureUrl, address } = this.form.getRawValue();
    this.auth
      .updateProfile({
        displayName,
        dateOfBirth,
        phoneNumber: phoneNumber.trim() === '' ? null : phoneNumber.trim(),
        profilePictureUrl: profilePictureUrl.trim() === '' ? null : profilePictureUrl.trim(),
        address: this.toAddress(address),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (profile) => {
          this.apply(profile);
          this.saving.set(false);
          this.toastr.success(this.translate.instant('profile.saved'));
        },
        error: (error: unknown) => {
          this.saving.set(false);
          if (applyServerErrors(this.form, error)) this.activeTab.set(this.firstTabWithErrors());
        },
      });
  }

  private firstTabWithErrors(): ProfileTab {
    const personalInvalid = ['displayName', 'dateOfBirth', 'phoneNumber', 'profilePictureUrl'].some(
      (name) => this.form.get(name)?.invalid,
    );
    return personalInvalid ? ProfileTab.Personal : ProfileTab.Address;
  }

  private toAddress(value: Record<string, string>): UserAddress | null {
    if (Object.values(value).every((field) => field.trim() === '')) return null;
    return {
      postalCode: value['postalCode'].trim(),
      street: value['street'].trim(),
      number: value['number'].trim(),
      complement: value['complement'].trim() === '' ? null : value['complement'].trim(),
      neighborhood: value['neighborhood'].trim(),
      city: value['city'].trim(),
      state: value['state'].trim(),
      country: value['country'].trim().toUpperCase(),
    };
  }

  private apply(profile: UserProfile): void {
    this.profile.set(profile);
    this.form.reset({
      displayName: profile.displayName,
      dateOfBirth: profile.dateOfBirth ?? '',
      phoneNumber: profile.phoneNumber ?? '',
      profilePictureUrl: profile.profilePictureUrl ?? '',
      address: {
        postalCode: profile.address?.postalCode ?? '',
        street: profile.address?.street ?? '',
        number: profile.address?.number ?? '',
        complement: profile.address?.complement ?? '',
        neighborhood: profile.address?.neighborhood ?? '',
        city: profile.address?.city ?? '',
        state: profile.address?.state ?? '',
        country: profile.address?.country ?? '',
      },
    });
    this.picturePreview.set(profile.profilePictureUrl ?? '');
    this.pictureFailed.set(false);
    if (!this.canUpdate()) this.form.disable();
  }

  protected onPostalInput(event: Event): void {
    const element = event.target as HTMLInputElement;
    const format = postalCodeFormat(this.selectedCountry());
    const value = format ? applyMask(element.value, format.pattern) : element.value.toUpperCase();
    element.value = value;
    this.form.controls.address.controls.postalCode.setValue(value);
  }

  protected selectTab(tab: ProfileTab): void {
    this.activeTab.set(tab);
  }

  protected onPictureError(): void {
    this.pictureFailed.set(true);
  }
}
