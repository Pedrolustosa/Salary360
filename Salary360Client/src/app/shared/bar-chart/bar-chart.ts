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
import {
  BarController,
  BarElement,
  CategoryScale,
  Chart,
  Legend,
  LinearScale,
  Tooltip,
} from 'chart.js';
import { ThemeMode } from '../../core/theme/enums/theme-mode.enum';
import { ThemeService } from '../../core/theme/theme.service';
import { BarChartDataset } from './interfaces/bar-chart-dataset.interface';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Legend, Tooltip);

@Component({
  selector: 'app-bar-chart',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './bar-chart.html',
  styleUrl: './bar-chart.scss',
})
export class BarChart {
  readonly labels = input.required<string[]>();
  readonly datasets = input.required<BarChartDataset[]>();
  readonly yTickFormatter = input<(value: number) => string>((value) => String(value));
  readonly ariaLabel = input('');

  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private readonly theme = inject(ThemeService);
  private chart?: Chart<'bar'>;

  constructor() {
    effect(() => {
      const labels = this.labels();
      const datasets = this.datasets();
      const formatter = this.yTickFormatter();
      const dark = this.theme.current() === ThemeMode.Dark;
      const legendColor = dark ? '#cbd5e1' : '#334155';
      const gridColor = dark ? 'rgba(148, 163, 184, 0.15)' : 'rgba(148, 163, 184, 0.25)';

      const chartDatasets = datasets.map((dataset) => ({
        label: dataset.label,
        data: dataset.data,
        backgroundColor: dataset.color,
        borderWidth: 0,
        stack: 'expenses',
      }));

      if (!this.chart) {
        this.chart = new Chart(this.canvas().nativeElement, {
          type: 'bar',
          data: { labels, datasets: chartDatasets },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
              legend: { position: 'bottom', labels: { color: legendColor } },
            },
            scales: {
              x: {
                stacked: true,
                ticks: { color: legendColor },
                grid: { color: gridColor },
              },
              y: {
                stacked: true,
                ticks: {
                  color: legendColor,
                  callback: (value) => formatter(Number(value)),
                },
                grid: { color: gridColor },
              },
            },
          },
        });
        return;
      }

      this.chart.data.labels = labels;
      this.chart.data.datasets = chartDatasets;
      this.chart.options.plugins!.legend!.labels!.color = legendColor;
      const x = this.chart.options.scales!['x'];
      const y = this.chart.options.scales!['y'];
      if (x && 'ticks' in x && x.ticks) x.ticks.color = legendColor;
      if (x && 'grid' in x && x.grid) x.grid.color = gridColor;
      if (y && 'ticks' in y && y.ticks) {
        y.ticks.color = legendColor;
        y.ticks.callback = (value) => formatter(Number(value));
      }
      if (y && 'grid' in y && y.grid) y.grid.color = gridColor;
      this.chart.update();
    });

    inject(DestroyRef).onDestroy(() => this.chart?.destroy());
  }
}
