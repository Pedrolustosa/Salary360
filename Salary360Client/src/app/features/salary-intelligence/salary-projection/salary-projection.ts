import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { formatMoney } from '../../../core/format/money';
import { LanguageService } from '../../../core/i18n/language.service';
import { LineChart } from '../../../shared/line-chart/line-chart';
import { LineChartDataset } from '../../../shared/line-chart/interfaces/line-chart-dataset.interface';
import { Skeleton } from '../../../shared/skeleton/skeleton';
import { StatCard } from '../../../shared/stat-card/stat-card';
import { StatTone } from '../../../shared/stat-card/enums/stat-tone.enum';
import { ProjectionScenario } from '../interfaces/projection-scenario.interface';
import { SalaryProjection } from '../interfaces/salary-projection.interface';
import { toProjectionChartData } from '../utils/salary-intelligence-charts';

const HORIZONS = [3, 6, 12] as const;

@Component({
  selector: 'app-salary-projection',
  imports: [RouterLink, TranslatePipe, LineChart, Skeleton, StatCard],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './salary-projection.html',
  styleUrl: './salary-projection.scss',
})
export class SalaryProjectionComponent {
  private readonly language = inject(LanguageService);
  private readonly translate = inject(TranslateService);

  readonly data = input<SalaryProjection | null>(null);
  readonly loading = input(false);
  readonly error = input(false);
  readonly horizon = input(6);
  readonly retry = output<void>();
  readonly horizonChange = output<number>();

  protected readonly StatTone = StatTone;
  protected readonly horizons = HORIZONS;

  protected readonly chart = computed(() => toProjectionChartData(this.data()?.series ?? []));

  protected readonly datasets = computed<LineChartDataset[]>(() => {
    this.language.current();
    const chart = this.chart();
    return [
      { label: this.translate.instant('salaryIntelligence.projection.actual'), data: chart.actual, color: '#0ea5e9' },
      {
        label: this.translate.instant('salaryIntelligence.projection.scenarios.conservative'),
        data: chart.conservative,
        color: '#64748b',
        borderDash: [6, 4],
      },
      {
        label: this.translate.instant('salaryIntelligence.projection.scenarios.likely'),
        data: chart.likely,
        color: '#059669',
        borderDash: [6, 4],
      },
      {
        label: this.translate.instant('salaryIntelligence.projection.scenarios.optimistic'),
        data: chart.optimistic,
        color: '#8b5cf6',
        borderDash: [6, 4],
      },
    ];
  });

  protected readonly scenarios = computed(() => this.data()?.scenarios ?? []);

  protected readonly yFormatter = (value: number): string => formatMoney(value, this.language.current());

  protected readonly isEmpty = computed(() => {
    const projection = this.data();
    return !!projection && !projection.hasEnoughData && projection.recordCount === 0;
  });

  protected readonly isInsufficient = computed(() => {
    const projection = this.data();
    return !!projection && !projection.hasEnoughData && projection.recordCount > 0;
  });

  protected readonly hasData = computed(() => !!this.data()?.hasEnoughData);

  protected money(value: number | null | undefined): string {
    return value === null || value === undefined ? '—' : formatMoney(value, this.language.current());
  }

  protected scenarioLabel(scenario: ProjectionScenario): string {
    return this.translate.instant(`salaryIntelligence.projection.scenarios.${scenario.name}`);
  }

  protected scenarioTone(name: string): StatTone {
    switch (name) {
      case 'optimistic':
        return StatTone.Violet;
      case 'likely':
        return StatTone.Emerald;
      default:
        return StatTone.Slate;
    }
  }

  protected setHorizon(value: number): void {
    if (value === this.horizon()) return;
    this.horizonChange.emit(value);
  }
}
