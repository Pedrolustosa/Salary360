import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { SegmentedControl } from './segmented-control';

describe('SegmentedControl', () => {
  let fixture: ComponentFixture<SegmentedControl>;
  let component: SegmentedControl;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SegmentedControl],
      providers: [provideTranslateService({ fallbackLang: 'pt-BR' })],
    }).compileComponents();

    fixture = TestBed.createComponent(SegmentedControl);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('options', [
      { value: 'Fixed', labelKey: 'expenses.type.Fixed' },
      { value: 'Variable', labelKey: 'expenses.type.Variable' },
    ]);
    fixture.detectChanges();
  });

  it('should write and emit values via CVA', () => {
    let value = '';
    component.registerOnChange((next) => (value = next));
    component.writeValue('Fixed');
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('button');
    expect(buttons[0].getAttribute('aria-checked')).toBe('true');

    buttons[1].click();
    fixture.detectChanges();
    expect(value).toBe('Variable');
  });

  it('should move selection with keyboard', () => {
    let value = '';
    component.registerOnChange((next) => (value = next));
    component.writeValue('Fixed');
    fixture.detectChanges();

    const group = fixture.nativeElement.querySelector('[role="radiogroup"]') as HTMLElement;
    group.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowRight' }));
    expect(value).toBe('Variable');
  });
});
