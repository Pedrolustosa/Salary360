import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { formatMoney } from '../../../core/format/money';
import { LanguageService } from '../../../core/i18n/language.service';
import { LineChart } from '../../../shared/line-chart/line-chart';
import { LineChartDataset } from '../../../shared/line-chart/interfaces/line-chart-dataset.interface';
import { Skeleton } from '../../../shared/skeleton/skeleton';
import { PurchasingPower } from '../interfaces/purchasing-power.interface';
import { competenceLabel, toPurchasingPowerChartData } from '../utils/salary-intelligence-charts';

@Component({
  selector: 'app-purchasing-power',
  imports: [RouterLink, TranslatePipe, LineChart, Skeleton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './purchasing-power.html',
  styleUrl: './purchasing-power.scss',
})
export class PurchasingPowerComponent {
  private readonly language = inject(LanguageService);
  private readonly translate = inject(TranslateService);

  readonly data = input<PurchasingPower | null>(null);
  readonly loading = input(false);
  readonly error = input(false);
  readonly retry = output<void>();

  protected readonly chart = computed(() => toPurchasingPowerChartData(this.data()?.series ?? []));

  protected readonly datasets = computed<LineChartDataset[]>(() => {
    this.language.current();
    const chart = this.chart();
    return [
      { label: this.translate.instant('salaryIntelligence.purchasingPower.actual'), data: chart.actual, color: '#0ea5e9' },
      {
        label: this.translate.instant('salaryIntelligence.purchasingPower.adjusted'),
        data: chart.inflationAdjusted,
        color: '#f59e0b',
        borderDash: [6, 4],
      },
    ];
  });

  protected readonly yFormatter = (value: number): string => formatMoney(value, this.language.current());

  protected readonly warningKey = computed(() => {
    const power = this.data();
    if (!power || !power.hasEnoughData) return null;
    if (!power.inflationAvailable) return 'unavailable';
    if (power.isStale) return 'stale';
    if (power.inflationCoverage?.isPartial) return 'partial';
    return null;
  });

  protected readonly partialUntil = computed(() => {
    const coverage = this.data()?.inflationCoverage;
    if (!coverage) return '';
    return competenceLabel(coverage.toYear, coverage.toMonth);
  });

  protected readonly isEmpty = computed(() => {
    const power = this.data();
    return !!power && !power.hasEnoughData && power.recordCount === 0;
  });

  protected readonly isInsufficient = computed(() => {
    const power = this.data();
    return !!power && !power.hasEnoughData && power.recordCount > 0;
  });

  protected readonly hasData = computed(() => !!this.data()?.hasEnoughData && (this.data()?.series.length ?? 0) > 0);
}
