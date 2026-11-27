import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatCard } from './stat-card';
import { StatTone } from './enums/stat-tone.enum';

describe('StatCard', () => {
  let fixture: ComponentFixture<StatCard>;

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  beforeEach(() => {
    fixture = TestBed.createComponent(StatCard);
    fixture.componentRef.setInput('label', 'Active');
    fixture.componentRef.setInput('value', 7);
    fixture.componentRef.setInput('icon', 'check-circle');
  });

  it('should render the label and the value', async () => {
    await fixture.whenStable();

    expect(element().textContent).toContain('Active');
    expect(element().textContent).toContain('7');
  });

  it('should not render the progress bar without a percent', async () => {
    await fixture.whenStable();

    expect(element().querySelector('.h-1\\.5')).toBeNull();
  });

  it('should render a clamped progress bar and the hint', async () => {
    fixture.componentRef.setInput('percent', 150);
    fixture.componentRef.setInput('hint', '67% of total');
    await fixture.whenStable();

    const bar = element().querySelector<HTMLElement>('.h-full.rounded-full');
    expect(bar?.style.width).toBe('100%');
    expect(element().textContent).toContain('67% of total');
  });

  it('should apply the classes of the tone', async () => {
    fixture.componentRef.setInput('tone', StatTone.Emerald);
    fixture.componentRef.setInput('percent', 50);
    await fixture.whenStable();

    expect(element().querySelector('.bg-emerald-500')).not.toBeNull();
    expect(element().querySelector('.text-emerald-700')).not.toBeNull();
  });
});
