import { CanDeactivateFn } from '@angular/router';
import { CanDeactivateComponent } from './interfaces/can-deactivate-component.interface';

export const canDeactivateGuard: CanDeactivateFn<CanDeactivateComponent> = (component) =>
  component.canDeactivate ? component.canDeactivate() : true;
