import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { formatMoney } from '../../../core/format/money';
import { LanguageService } from '../../../core/i18n/language.service';
import { Skeleton } from '../../../shared/skeleton/skeleton';
import { StatCard } from '../../../shared/stat-card/stat-card';
import { StatTone } from '../../../shared/stat-card/enums/stat-tone.enum';
import { SalaryBasis } from '../enums/salary-basis.enum';
import { SalaryIntelligenceSummary } from '../interfaces/salary-intelligence-summary.interface';

@Component({
  selector: 'app-salary-intelligence-summary',
  imports: [RouterLink, TranslatePipe, Skeleton, StatCard],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './salary-intelligence-summary.html',
  styleUrl: './salary-intelligence-summary.scss',
})
export class SalaryIntelligenceSummaryComponent {
  private readonly language = inject(LanguageService);

  readonly data = input<SalaryIntelligenceSummary | null>(null);
  readonly basis = input(SalaryBasis.Net);
  readonly loading = input(false);
  readonly error = input(false);
  readonly retry = output<void>();

  protected readonly StatTone = StatTone;

  protected readonly money = (value: number | null): string =>
    value === null ? '—' : formatMoney(value, this.language.current());

  private readonly evolutionPercent = computed(() => {
    const summary = this.data();
    if (!summary) return null;
    return this.basis() === SalaryBasis.Gross
      ? summary.grossChangePercent
      : summary.netChangePercent;
  });

  protected readonly evolutionDisplay = computed(() => {
    this.language.current();
    const value = this.evolutionPercent();
    if (value === null) return '—';
    const formatted = new Intl.NumberFormat(this.language.current(), {
      maximumFractionDigits: 1,
      signDisplay: 'exceptZero',
    }).format(value);
    return `${value > 0 ? '↑ ' : value < 0 ? '↓ ' : ''}${formatted}%`;
  });

  protected readonly evolutionTone = computed(() => {
    const value = this.evolutionPercent();
    if (value === null) return StatTone.Slate;
    if (value > 0) return StatTone.Emerald;
    if (value < 0) return StatTone.Violet;
    return StatTone.Slate;
  });

  protected readonly isEmpty = computed(() => {
    const summary = this.data();
    return !!summary && !summary.hasEnoughData && summary.recordCount === 0;
  });

  protected readonly isInsufficient = computed(() => {
    const summary = this.data();
    return !!summary && !summary.hasEnoughData && summary.recordCount > 0;
  });

  protected readonly hasData = computed(() => !!this.data()?.hasEnoughData);

  protected onRetry(): void {
    this.retry.emit();
  }
}

