import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { RouteLoader } from './shared/route-loader';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouteLoader],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
