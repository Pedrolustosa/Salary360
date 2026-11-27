import { ChangeDetectionStrategy, Component } from '@angular/core';
import { LanguageSwitcher } from './language-switcher';
import { ThemeToggle } from './theme-toggle';

@Component({
  selector: 'app-toolbar',
  imports: [LanguageSwitcher, ThemeToggle],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './toolbar.html',
  styleUrl: './toolbar.scss',
})
export class Toolbar {}
