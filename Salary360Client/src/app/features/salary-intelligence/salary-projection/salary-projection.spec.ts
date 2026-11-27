import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { SalaryProjection } from '../interfaces/salary-projection.interface';
import { SalaryProjectionComponent } from './salary-projection';

describe('SalaryProjectionComponent', () => {
  let fixture: ComponentFixture<SalaryProjectionComponent>;

  function root(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalaryProjectionComponent],
      providers: [provideTranslateService({ fallbackLang: 'pt-BR' }), provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(SalaryProjectionComponent);
  });

  it('should always show disclaimer', () => {
    fixture.detectChanges();
    expect(root().textContent).toContain('salaryIntelligence.projection.disclaimer');
  });

  it('should show insufficient state', () => {
    const data: SalaryProjection = {
      hasEnoughData: false,
      requiredRecords: 6,
      recordCount: 2,
      from: null,
      to: null,
      method: null,
      sampleSize: null,
      horizonMonths: null,
      disclaimerCode: null,
      inflationCriterion: null,
      scenarios: [],
      series: [],
    };
    fixture.componentRef.setInput('data', data);
    fixture.detectChanges();
    expect(root().textContent).toContain('salaryIntelligence.states.insufficientRecords');
  });

  it('should emit horizon changes', () => {
    const horizons: number[] = [];
    fixture.componentRef.setInput('horizon', 6);
    fixture.componentInstance.horizonChange.subscribe((value) => horizons.push(value));
    fixture.detectChanges();

    const buttons = Array.from(root().querySelectorAll<HTMLButtonElement>('button'));
    const three = buttons.find((button) => button.textContent?.includes('3'));
    three!.click();
    expect(horizons).toEqual([3]);
  });

  it('should render scenario cards', () => {
    const data: SalaryProjection = {
      hasEnoughData: true,
      requiredRecords: 6,
      recordCount: 8,
      from: null,
      to: null,
      method: 'ols_linear',
      sampleSize: 8,
      horizonMonths: 6,
      disclaimerCode: 'projection_estimate',
      inflationCriterion: null,
      scenarios: [
        { name: 'conservative', value: 900, realValue: null },
        { name: 'likely', value: 1000, realValue: 980 },
        { name: 'optimistic', value: 1100, realValue: null },
      ],
      series: [{ year: 2024, month: 1, actual: 1000, conservative: null, likely: null, optimistic: null }],
    };
    fixture.componentRef.setInput('data', data);
    fixture.detectChanges();
    expect(root().querySelectorAll('app-stat-card')).toHaveLength(3);
  });
});
