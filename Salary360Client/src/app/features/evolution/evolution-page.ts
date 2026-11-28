import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { catchError, debounceTime, of, switchMap, tap } from 'rxjs';
import { formatMoney } from '../../core/format/money';
import { LanguageService } from '../../core/i18n/language.service';
import { LineChart } from '../../shared/line-chart/line-chart';
import { LineChartDataset } from '../../shared/line-chart/interfaces/line-chart-dataset.interface';
import { Select } from '../../shared/select/select';
import { SelectOption } from '../../shared/select/interfaces/select-option.interface';
import { Spinner } from '../../shared/spinner';
import { StatCard } from '../../shared/stat-card/stat-card';
import { StatTone } from '../../shared/stat-card/enums/stat-tone.enum';
import { EvolutionMode } from '../../core/salaries/enums/evolution-mode.enum';
import { SalaryEvolutionPoint } from '../../core/salaries/interfaces/salary-evolution-point.interface';
import { SalaryService } from '../../core/salaries/salary.service';
import {
  computeEvolutionStats,
  toEvolutionChartData,
  validateEvolutionMonth,
  validateEvolutionRange,
} from './utils/evolution-chart';

@Component({
  selector: 'app-evolution-page',
  imports: [FormsModule, TranslatePipe, Select, LineChart, Spinner, StatCard],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './evolution-page.html',
  styleUrl: './evolution-page.scss',
})
export class EvolutionPage {
  private readonly salaries = inject(SalaryService);
  private readonly language = inject(LanguageService);
  private readonly translate = inject(TranslateService);

  protected readonly EvolutionMode = EvolutionMode;
  protected readonly StatTone = StatTone;

  private readonly now = new Date();
  protected readonly mode = signal(EvolutionMode.Month);
  protected readonly month = signal(String(this.now.getMonth() + 1));
  protected readonly fromYear = signal(String(this.now.getFullYear() - 4));
  protected readonly toYear = signal(String(this.now.getFullYear()));
  protected readonly loading = signal(false);
  protected readonly filterError = signal<string | null>(null);

  protected readonly monthOptions = computed<SelectOption[]>(() => {
    const formatter = new Intl.DateTimeFormat(this.language.current(), { month: 'long' });
    return Array.from({ length: 12 }, (_, index) => ({
      value: String(index + 1),
      label: formatter.format(new Date(2024, index, 1)).replace(/^./, (char) => char.toUpperCase()),
    }));
  });

  protected readonly yearOptions = computed<SelectOption[]>(() => {
    const current = this.now.getFullYear();
    return Array.from({ length: current - 1900 + 1 }, (_, index) => {
      const year = current - index;
      return { value: String(year), label: String(year) };
    });
  });

  private readonly query = computed(() => ({
    mode: this.mode(),
    month: Number(this.month()),
    fromYear: Number(this.fromYear()),
    toYear: Number(this.toYear()),
  }));

  private readonly result = toSignal(
    toObservable(this.query).pipe(
      debounceTime(300),
      tap((query) => {
        const rangeError = validateEvolutionRange(query.fromYear, query.toYear);
        const monthError = query.mode === EvolutionMode.Month ? validateEvolutionMonth(query.month) : null;
        const error = rangeError ?? monthError;
        this.filterError.set(error);
        this.loading.set(!error);
      }),
      switchMap((query) => {
        if (this.filterError()) return of([] as SalaryEvolutionPoint[]);
        return this.salaries
          .evolution({
            mode: query.mode,
            month: query.mode === EvolutionMode.Month ? query.month : null,
            fromYear: query.fromYear,
            toYear: query.toYear,
          })
          .pipe(
            tap(() => this.loading.set(false)),
            catchError(() => {
              this.loading.set(false);
              return of([] as SalaryEvolutionPoint[]);
            }),
          );
      }),
    ),
    { initialValue: [] as SalaryEvolutionPoint[] },
  );

  protected readonly chart = computed(() => toEvolutionChartData(this.result()));
  protected readonly stats = computed(() => computeEvolutionStats(this.result()));
  protected readonly hasData = computed(() => this.result().some((point) => point.net !== null));

  protected readonly datasets = computed<LineChartDataset[]>(() => {
    const chart = this.chart();
    return [
      { label: this.translate.instant('salaries.net'), data: chart.net, color: '#059669' },
      { label: this.translate.instant('salaries.gross'), data: chart.gross, color: '#0ea5e9' },
      { label: this.translate.instant('salaries.benefits'), data: chart.benefits, color: '#10b981', hidden: true },
      { label: this.translate.instant('salaries.deductions'), data: chart.deductions, color: '#f43f5e', hidden: true },
    ];
  });

  protected readonly yFormatter = (value: number): string => formatMoney(value, this.language.current());

  protected readonly maxNetDisplay = computed(() => {
    const value = this.stats().maxNet;
    return value === null ? '—' : formatMoney(value, this.language.current());
  });

  protected readonly minNetDisplay = computed(() => {
    const value = this.stats().minNet;
    return value === null ? '—' : formatMoney(value, this.language.current());
  });

  protected readonly variationDisplay = computed(() => {
    const value = this.stats().variationPercent;
    if (value === null) return '—';
    const formatted = new Intl.NumberFormat(this.language.current(), {
      maximumFractionDigits: 1,
      signDisplay: 'exceptZero',
    }).format(value);
    return `${formatted}%`;
  });

  protected setMode(mode: EvolutionMode): void {
    this.mode.set(mode);
  }
}
