import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Icon } from '../../../shared/icon';
import { IconName } from '../../../shared/icon-paths';
import { Skeleton } from '../../../shared/skeleton/skeleton';
import { InsightSeverity } from '../enums/insight-severity.enum';
import { SalaryInsight } from '../interfaces/salary-insight.interface';
import { SalaryInsights } from '../interfaces/salary-insights.interface';

@Component({
  selector: 'app-salary-insights',
  imports: [RouterLink, TranslatePipe, Icon, Skeleton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './salary-insights.html',
  styleUrl: './salary-insights.scss',
})
export class SalaryInsightsComponent {
  readonly data = input<SalaryInsights | null>(null);
  readonly loading = input(false);
  readonly error = input(false);
  readonly retry = output<void>();

  protected readonly insights = computed(() => this.data()?.insights ?? []);

  protected readonly isEmpty = computed(() => {
    const insights = this.data();
    return !!insights && !insights.hasEnoughData && insights.recordCount === 0;
  });

  protected readonly isInsufficient = computed(() => {
    const insights = this.data();
    return !!insights && !insights.hasEnoughData && insights.recordCount > 0;
  });

  protected readonly hasData = computed(() => !!this.data()?.hasEnoughData);

  protected iconFor(severity: InsightSeverity): IconName {
    switch (severity) {
      case InsightSeverity.Positive:
        return 'check-circle';
      case InsightSeverity.Warning:
        return 'exclamation-triangle';
      default:
        return 'information-circle';
    }
  }

  protected toneClass(severity: InsightSeverity): string {
    switch (severity) {
      case InsightSeverity.Positive:
        return 'border-emerald-200 bg-emerald-50 text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950/40 dark:text-emerald-200';
      case InsightSeverity.Warning:
        return 'border-amber-200 bg-amber-50 text-amber-800 dark:border-amber-900 dark:bg-amber-950/40 dark:text-amber-200';
      default:
        return 'border-slate-200 bg-slate-50 text-slate-700 dark:border-slate-700 dark:bg-slate-800/60 dark:text-slate-200';
    }
  }

  protected params(insight: SalaryInsight): Record<string, unknown> {
    return insight.params ?? {};
  }
}
