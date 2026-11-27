import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  computed,
  forwardRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Icon } from '../icon';
import { SelectOption } from './interfaces/select-option.interface';

@Component({
  selector: 'app-select',
  imports: [Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './select.html',
  styleUrl: './select.scss',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => Select), multi: true }],
})
export class Select implements ControlValueAccessor {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroyRef = inject(DestroyRef);

  readonly options = input.required<SelectOption[]>();
  readonly inputId = input('select');
  readonly placeholder = input('');
  readonly searchPlaceholder = input('');
  readonly emptyLabel = input('');
  readonly ariaLabel = input('');
  readonly searchable = input(false);
  readonly compact = input(false);

  protected readonly value = signal('');
  protected readonly open = signal(false);
  protected readonly query = signal('');
  protected readonly disabled = signal(false);
  protected readonly panelTop = signal<number | null>(null);
  protected readonly panelBottom = signal<number | null>(null);
  protected readonly panelLeft = signal(0);
  protected readonly panelWidth = signal(0);

  protected readonly selected = computed(() => this.options().find((option) => option.value === this.value()));
  protected readonly filtered = computed(() => {
    const term = this.query().trim().toLowerCase();
    if (!term) return this.options();
    return this.options().filter((option) => option.label.toLowerCase().includes(term));
  });

  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;
  private readonly onScroll = (): void => {
    if (this.open()) this.reposition();
  };

  constructor() {
    document.addEventListener('scroll', this.onScroll, true);
    this.destroyRef.onDestroy(() => document.removeEventListener('scroll', this.onScroll, true));
  }

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (this.open() && !this.host.nativeElement.contains(event.target as Node)) this.close();
  }

  @HostListener('keydown.escape')
  protected onEscape(): void {
    if (this.open()) this.close();
  }

  @HostListener('window:resize')
  protected onResize(): void {
    if (this.open()) this.reposition();
  }

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
    if (isDisabled) this.open.set(false);
  }

  protected toggle(): void {
    if (this.disabled()) return;
    if (this.open()) {
      this.close();
      return;
    }
    this.query.set('');
    this.reposition();
    this.open.set(true);
  }

  protected choose(option: SelectOption): void {
    this.value.set(option.value);
    this.onChange(option.value);
    this.close();
  }

  protected onSearch(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }

  private reposition(): void {
    const trigger = this.host.nativeElement.querySelector('button');
    if (!trigger) return;

    const rect = trigger.getBoundingClientRect();
    const panelMaxHeight = 224;
    const gap = 4;
    const spaceBelow = window.innerHeight - rect.bottom - gap;
    const spaceAbove = rect.top - gap;
    const openUpward = spaceBelow < panelMaxHeight && spaceAbove > spaceBelow;

    this.panelLeft.set(rect.left);
    this.panelWidth.set(Math.max(rect.width, 192));

    if (openUpward) {
      this.panelTop.set(null);
      this.panelBottom.set(window.innerHeight - rect.top + gap);
    } else {
      this.panelTop.set(rect.bottom + gap);
      this.panelBottom.set(null);
    }
  }

  private close(): void {
    this.open.set(false);
    this.query.set('');
    this.onTouched();
  }
}
