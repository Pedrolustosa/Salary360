import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { formatMoney } from '../../../core/format/money';
import { LanguageService } from '../../../core/i18n/language.service';
import { LineChart } from '../../../shared/line-chart/line-chart';
import { LineChartDataset } from '../../../shared/line-chart/interfaces/line-chart-dataset.interface';
import { Skeleton } from '../../../shared/skeleton/skeleton';
import { SalarySeries } from '../interfaces/salary-series.interface';
import { toEvolutionChartData } from '../utils/salary-intelligence-charts';

@Component({
  selector: 'app-salary-evolution-chart',
  imports: [RouterLink, TranslatePipe, LineChart, Skeleton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './salary-evolution-chart.html',
  styleUrl: './salary-evolution-chart.scss',
})
export class SalaryEvolutionChart {
  private readonly language = inject(LanguageService);
  private readonly translate = inject(TranslateService);

  readonly data = input<SalarySeries | null>(null);
  readonly loading = input(false);
  readonly error = input(false);
  readonly retry = output<void>();

  protected readonly chart = computed(() => toEvolutionChartData(this.data()?.points ?? []));

  protected readonly datasets = computed<LineChartDataset[]>(() => {
    this.language.current();
    const chart = this.chart();
    return [
      { label: this.translate.instant('salaryIntelligence.charts.gross'), data: chart.gross, color: '#0ea5e9' },
      { label: this.translate.instant('salaryIntelligence.charts.net'), data: chart.net, color: '#059669' },
    ];
  });

  protected readonly yFormatter = (value: number): string => formatMoney(value, this.language.current());

  protected readonly isEmpty = computed(() => {
    const series = this.data();
    return !!series && !series.hasEnoughData && series.recordCount === 0;
  });

  protected readonly isInsufficient = computed(() => {
    const series = this.data();
    return !!series && !series.hasEnoughData && series.recordCount > 0;
  });

  protected readonly hasData = computed(() => !!this.data()?.hasEnoughData && (this.data()?.points.length ?? 0) > 0);
}
