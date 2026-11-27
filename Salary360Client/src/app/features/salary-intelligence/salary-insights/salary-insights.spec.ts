import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { InsightSeverity } from '../enums/insight-severity.enum';
import { SalaryInsights } from '../interfaces/salary-insights.interface';
import { SalaryInsightsComponent } from './salary-insights';

describe('SalaryInsightsComponent', () => {
  let fixture: ComponentFixture<SalaryInsightsComponent>;

  function root(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalaryInsightsComponent],
      providers: [provideTranslateService({ fallbackLang: 'pt-BR' }), provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(SalaryInsightsComponent);
  });

  it('should show loading skeleton', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    expect(root().querySelector('app-skeleton')).not.toBeNull();
  });

  it('should show error state', () => {
    fixture.componentRef.setInput('error', true);
    fixture.detectChanges();
    expect(root().textContent).toContain('salaryIntelligence.states.error');
  });

  it('should render insights by severity', () => {
    const data: SalaryInsights = {
      hasEnoughData: true,
      requiredRecords: 2,
      recordCount: 4,
      from: null,
      to: null,
      insights: [
        { code: 'salary_increase', severity: InsightSeverity.Positive, params: { percent: 5 }, referenceCompetence: null },
        { code: 'stable_period', severity: InsightSeverity.Info, params: {}, referenceCompetence: null },
      ],
    };
    fixture.componentRef.setInput('data', data);
    fixture.detectChanges();
    expect(root().querySelectorAll('li')).toHaveLength(2);
  });
});
