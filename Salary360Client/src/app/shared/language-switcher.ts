import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageCode } from '../core/i18n/enums/language-code.enum';
import { LanguageService } from '../core/i18n/language.service';
import { SelectOption } from './select/interfaces/select-option.interface';
import { Select } from './select/select';

@Component({
  selector: 'app-language-switcher',
  imports: [ReactiveFormsModule, TranslatePipe, Select],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './language-switcher.html',
  styleUrl: './language-switcher.scss',
})
export class LanguageSwitcher {
  private readonly destroyRef = inject(DestroyRef);
  protected readonly language = inject(LanguageService);
  protected readonly control = new FormControl<string>(this.language.current(), { nonNullable: true });
  protected readonly options: SelectOption[] = this.language.languages.map((item) => ({
    value: item.code,
    label: item.label,
  }));

  constructor() {
    effect(() => this.control.setValue(this.language.current(), { emitEvent: false }));
    this.control.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((code) => void this.language.set(code as LanguageCode));
  }
}
