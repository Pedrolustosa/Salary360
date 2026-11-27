import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-skeleton',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './skeleton.html',
  styleUrl: './skeleton.scss',
})
export class Skeleton {
  readonly lines = input(3);
  readonly className = input('', { alias: 'class' });

  protected readonly items = computed(() =>
    Array.from({ length: Math.max(1, this.lines()) }, (_, index) => index),
  );
}
