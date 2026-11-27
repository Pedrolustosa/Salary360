import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { SalaryIntelligenceSummary } from '../interfaces/salary-intelligence-summary.interface';
import { SalaryIntelligenceSummaryComponent } from './salary-intelligence-summary';

describe('SalaryIntelligenceSummaryComponent', () => {
  let fixture: ComponentFixture<SalaryIntelligenceSummaryComponent>;

  function root(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalaryIntelligenceSummaryComponent],
      providers: [provideTranslateService({ fallbackLang: 'pt-BR' }), provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(SalaryIntelligenceSummaryComponent);
  });

  it('should show skeleton while loading', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    expect(root().querySelector('app-skeleton')).not.toBeNull();
  });

  it('should show error and emit retry', () => {
    const retry = vi.fn();
    fixture.componentRef.setInput('error', true);
    fixture.componentInstance.retry.subscribe(retry);
    fixture.detectChanges();

    expect(root().textContent).toContain('salaryIntelligence.states.error');
    root().querySelector<HTMLButtonElement>('button')!.click();
    fixture.detectChanges();
    expect(retry).toHaveBeenCalledTimes(1);
  });

  it('should show empty state', () => {
    const data: SalaryIntelligenceSummary = {
      hasEnoughData: false,
      requiredRecords: 2,
      recordCount: 0,
      from: null,
      to: null,
      currentGross: null,
      currentNet: null,
      totalBenefits: null,
      totalDeductions: null,
      grossChangeAbsolute: null,
      grossChangePercent: null,
      netChangeAbsolute: null,
      netChangePercent: null,
    };
    fixture.componentRef.setInput('data', data);
    fixture.detectChanges();
    expect(root().textContent).toContain('salaryIntelligence.states.empty');
  });

  it('should render summary cards for data', () => {
    const data: SalaryIntelligenceSummary = {
      hasEnoughData: true,
      requiredRecords: 2,
      recordCount: 3,
      from: { year: 2024, month: 1 },
      to: { year: 2024, month: 3 },
      currentGross: 1000,
      currentNet: 800,
      totalBenefits: 100,
      totalDeductions: 50,
      grossChangeAbsolute: 100,
      grossChangePercent: 10,
      netChangeAbsolute: 80,
      netChangePercent: 11,
    };
    fixture.componentRef.setInput('data', data);
    fixture.detectChanges();
    expect(root().querySelectorAll('app-stat-card')).toHaveLength(4);
  });
});
