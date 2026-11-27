import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  effect,
  inject,
  input,
  viewChild,
} from '@angular/core';
import { ThemeService } from '../core/theme/theme.service';
import { ThemeMode } from '../core/theme/enums/theme-mode.enum';
import { ArcElement, Chart, DoughnutController, Legend, Tooltip } from 'chart.js';

Chart.register(DoughnutController, ArcElement, Tooltip, Legend);

const DEFAULT_COLORS = ['#059669', '#0ea5e9', '#8b5cf6', '#f59e0b', '#ec4899', '#cbd5e1'];

@Component({
  selector: 'app-doughnut-chart',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './doughnut-chart.html',
  styleUrl: './doughnut-chart.scss',
})
export class DoughnutChart {
  readonly labels = input.required<string[]>();
  readonly values = input.required<number[]>();
  readonly colors = input<string[]>(DEFAULT_COLORS);
  readonly ariaLabel = input<string>('');

  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private readonly theme = inject(ThemeService);
  private chart?: Chart<'doughnut'>;

  constructor() {
    effect(() => {
      const labels = this.labels();
      const values = this.values();
      const colors = this.colors();
      const legendColor = this.theme.current() === ThemeMode.Dark ? '#cbd5e1' : '#334155';

      if (!this.chart) {
        this.chart = new Chart(this.canvas().nativeElement, {
          type: 'doughnut',
          data: { labels, datasets: [{ data: values, backgroundColor: colors, borderWidth: 0 }] },
          options: { responsive: true, maintainAspectRatio: false, cutout: '65%', plugins: { legend: { position: 'bottom', labels: { color: legendColor } } } },
        });
        return;
      }

      this.chart.data.labels = labels;
      this.chart.data.datasets[0].data = values;
      this.chart.data.datasets[0].backgroundColor = colors;
      this.chart.options.plugins!.legend!.labels!.color = legendColor;
      this.chart.update();
    });

    inject(DestroyRef).onDestroy(() => this.chart?.destroy());
  }
}
