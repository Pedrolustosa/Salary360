import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { Subject, of } from 'rxjs';
import { SalaryService } from '../../../core/salaries/salary.service';
import { SalaryBasis } from '../enums/salary-basis.enum';
import { SalaryIntelligenceService } from '../services/salary-intelligence.service';
import { SalaryIntelligence } from './salary-intelligence';

describe('SalaryIntelligence', () => {
  let fixture: ComponentFixture<SalaryIntelligence>;
  const changed$ = new Subject<void>();
  const api = {
    summary: vi.fn(),
    series: vi.fn(),
    purchasingPower: vi.fn(),
    insights: vi.fn(),
    projection: vi.fn(),
  };
  const salaries = { changed$: changed$.asObservable() };

  const emptySummary = {
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

  beforeEach(async () => {
    vi.clearAllMocks();
    vi.useFakeTimers();
    api.summary.mockReturnValue(of(emptySummary));
    api.series.mockReturnValue(of({ hasEnoughData: false, requiredRecords: 2, recordCount: 0, from: null, to: null, points: [] }));
    api.purchasingPower.mockReturnValue(
      of({
        hasEnoughData: false,
        requiredRecords: 2,
        recordCount: 0,
        from: null,
        to: null,
        inflationAvailable: false,
        isStale: false,
        inflationCoverage: null,
        nominalChangePercent: null,
        ipcaAccumulatedPercent: null,
        realGainPercent: null,
        realGainAbsolute: null,
        series: [],
      }),
    );
    api.insights.mockReturnValue(
      of({ hasEnoughData: false, requiredRecords: 2, recordCount: 0, from: null, to: null, insights: [] }),
    );
    api.projection.mockReturnValue(
      of({
        hasEnoughData: false,
        requiredRecords: 6,
        recordCount: 0,
        from: null,
        to: null,
        method: null,
        sampleSize: null,
        horizonMonths: null,
        disclaimerCode: null,
        inflationCriterion: null,
        scenarios: [],
        series: [],
      }),
    );

    await TestBed.configureTestingModule({
      imports: [SalaryIntelligence],
      providers: [
        provideTranslateService({ fallbackLang: 'pt-BR' }),
        provideRouter([]),
        { provide: SalaryIntelligenceService, useValue: api },
        { provide: SalaryService, useValue: salaries },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SalaryIntelligence);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should load on init', async () => {
    fixture.detectChanges();
    await vi.advanceTimersByTimeAsync(300);
    expect(api.summary).toHaveBeenCalledWith({ from: null, to: null, basis: SalaryBasis.Net });
    expect(api.series).toHaveBeenCalled();
    expect(api.purchasingPower).toHaveBeenCalled();
    expect(api.insights).toHaveBeenCalled();
    expect(api.projection).toHaveBeenCalled();
  });

  it('should reload when salary changed$', async () => {
    fixture.detectChanges();
    await vi.advanceTimersByTimeAsync(300);
    const calls = api.summary.mock.calls.length;

    changed$.next();
    await vi.advanceTimersByTimeAsync(300);

    expect(api.summary.mock.calls.length).toBeGreaterThan(calls);
  });
});
