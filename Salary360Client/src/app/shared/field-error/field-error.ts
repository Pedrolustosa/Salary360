import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { of, startWith, switchMap } from 'rxjs';
import { serverErrorCode } from '../../core/forms/server-errors';

@Component({
  selector: 'app-field-error',
  imports: [TranslatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './field-error.html',
  styleUrl: './field-error.scss',
})
export class FieldError {
  readonly control = input.required<AbstractControl | null>();

  private readonly status = toSignal(
    toObservable(this.control).pipe(
      switchMap((control) => (control ? control.statusChanges.pipe(startWith(control.status)) : of(null))),
    ),
  );

  protected readonly code = computed(() => {
    this.status();
    return serverErrorCode(this.control());
  });
}
