import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { Icon } from '../icon';
import { IconName } from '../icon-paths';
import { STAT_TONES } from './constants/stat-tones';
import { StatTone } from './enums/stat-tone.enum';

@Component({
  selector: 'app-stat-card',
  imports: [Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './stat-card.html',
  styleUrl: './stat-card.scss',
})
export class StatCard {
  readonly label = input.required<string>();
  readonly value = input.required<number>();
  readonly icon = input.required<IconName>();
  readonly tone = input(StatTone.Brand);
  readonly percent = input<number | null>(null);
  readonly hint = input('');

  protected readonly classes = computed(() => STAT_TONES[this.tone()]);
  protected readonly width = computed(() => `${Math.min(100, Math.max(0, this.percent() ?? 0))}%`);
}
