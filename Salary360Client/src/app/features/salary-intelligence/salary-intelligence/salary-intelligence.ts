import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { Subject, catchError, debounceTime, merge, of, switchMap, tap } from 'rxjs';
import { SalaryService } from '../../salaries/services/salary.service';
import { SalaryBasis } from '../enums/salary-basis.enum';
import { PurchasingPower } from '../interfaces/purchasing-power.interface';
import { SalaryAnalysisFilter } from '../interfaces/salary-analysis-filter.interface';
import { SalaryInsights } from '../interfaces/salary-insights.interface';
import { SalaryIntelligenceSummary } from '../interfaces/salary-intelligence-summary.interface';
import { SalaryProjection } from '../interfaces/salary-projection.interface';
import { SalarySeries } from '../interfaces/salary-series.interface';
import { PurchasingPowerComponent } from '../purchasing-power/purchasing-power';
import { SalaryAnalysisFilters } from '../salary-analysis-filters/salary-analysis-filters';
import { SalaryEvolutionChart } from '../salary-evolution-chart/salary-evolution-chart';
import { SalaryInsightsComponent } from '../salary-insights/salary-insights';
import { SalaryIntelligenceSummaryComponent } from '../salary-intelligence-summary/salary-intelligence-summary';
import { SalaryProjectionComponent } from '../salary-projection/salary-projection';
import { SalaryIntelligenceService } from '../services/salary-intelligence.service';

@Component({
  selector: 'app-salary-intelligence',
  imports: [
    TranslatePipe,
    SalaryAnalysisFilters,
    SalaryIntelligenceSummaryComponent,
    SalaryEvolutionChart,
    PurchasingPowerComponent,
    SalaryInsightsComponent,
    SalaryProjectionComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './salary-intelligence.html',
  styleUrl: './salary-intelligence.scss',
})
export class SalaryIntelligence {
  private readonly api = inject(SalaryIntelligenceService);
  private readonly salaries = inject(SalaryService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly reload$ = new Subject<void>();

  protected readonly filter = signal<SalaryAnalysisFilter>({
    from: null,
    to: null,
    basis: SalaryBasis.Net,
  });
  protected readonly horizon = signal(6);

  protected readonly summary = signal<SalaryIntelligenceSummary | null>(null);
  protected readonly summaryLoading = signal(true);
  protected readonly summaryError = signal(false);

  protected readonly series = signal<SalarySeries | null>(null);
  protected readonly seriesLoading = signal(true);
  protected readonly seriesError = signal(false);

  protected readonly purchasingPower = signal<PurchasingPower | null>(null);
  protected readonly purchasingPowerLoading = signal(true);
  protected readonly purchasingPowerError = signal(false);

  protected readonly insights = signal<SalaryInsights | null>(null);
  protected readonly insightsLoading = signal(true);
  protected readonly insightsError = signal(false);

  protected readonly projection = signal<SalaryProjection | null>(null);
  protected readonly projectionLoading = signal(true);
  protected readonly projectionError = signal(false);

  private readonly query = computed(() => ({
    filter: this.filter(),
    horizon: this.horizon(),
  }));

  constructor() {
    const query$ = merge(toObservable(this.query), this.reload$, this.salaries.changed$).pipe(debounceTime(300));

    query$
      .pipe(
        tap(() => {
          this.summaryLoading.set(true);
          this.summaryError.set(false);
        }),
        switchMap(() =>
          this.api.summary(this.filter()).pipe(
            tap((data) => {
              this.summary.set(data);
              this.summaryLoading.set(false);
            }),
            catchError(() => {
              this.summary.set(null);
              this.summaryLoading.set(false);
              this.summaryError.set(true);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();

    query$
      .pipe(
        tap(() => {
          this.seriesLoading.set(true);
          this.seriesError.set(false);
        }),
        switchMap(() =>
          this.api.series(this.filter()).pipe(
            tap((data) => {
              this.series.set(data);
              this.seriesLoading.set(false);
            }),
            catchError(() => {
              this.series.set(null);
              this.seriesLoading.set(false);
              this.seriesError.set(true);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();

    query$
      .pipe(
        tap(() => {
          this.purchasingPowerLoading.set(true);
          this.purchasingPowerError.set(false);
        }),
        switchMap(() =>
          this.api.purchasingPower(this.filter()).pipe(
            tap((data) => {
              this.purchasingPower.set(data);
              this.purchasingPowerLoading.set(false);
            }),
            catchError(() => {
              this.purchasingPower.set(null);
              this.purchasingPowerLoading.set(false);
              this.purchasingPowerError.set(true);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();

    query$
      .pipe(
        tap(() => {
          this.insightsLoading.set(true);
          this.insightsError.set(false);
        }),
        switchMap(() =>
          this.api.insights(this.filter()).pipe(
            tap((data) => {
              this.insights.set(data);
              this.insightsLoading.set(false);
            }),
            catchError(() => {
              this.insights.set(null);
              this.insightsLoading.set(false);
              this.insightsError.set(true);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();

    query$
      .pipe(
        tap(() => {
          this.projectionLoading.set(true);
          this.projectionError.set(false);
        }),
        switchMap(() =>
          this.api.projection(this.filter(), this.horizon()).pipe(
            tap((data) => {
              this.projection.set(data);
              this.projectionLoading.set(false);
            }),
            catchError(() => {
              this.projection.set(null);
              this.projectionLoading.set(false);
              this.projectionError.set(true);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  protected onFilterChange(filter: SalaryAnalysisFilter): void {
    this.filter.set(filter);
  }

  protected onHorizonChange(horizon: number): void {
    this.horizon.set(horizon);
  }

  protected reload(): void {
    this.reload$.next();
  }
}
