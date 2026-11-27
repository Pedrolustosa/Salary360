import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { SKIP_ERROR_TOAST } from '../../../core/api/api-error';
import { environment } from '../../../../environments/environment';
import { SalaryBasis } from '../enums/salary-basis.enum';
import { SalaryIntelligenceService } from './salary-intelligence.service';

describe('SalaryIntelligenceService', () => {
  let service: SalaryIntelligenceService;
  let controller: HttpTestingController;
  const base = `${environment.apiUrl}/salary-intelligence`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SalaryIntelligenceService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should request summary with filters and skip error toast', () => {
    service.summary({ from: '2024-01', to: '2024-06', basis: SalaryBasis.Gross }).subscribe();
    const req = controller.expectOne(`${base}/summary?basis=Gross&from=2024-01&to=2024-06`);
    expect(req.request.method).toBe('GET');
    expect(req.request.context.get(SKIP_ERROR_TOAST)).toBe(true);
    req.flush({ hasEnoughData: false, requiredRecords: 2, recordCount: 0 });
  });

  it('should request series, purchasing power and insights', () => {
    const filter = { from: null, to: null, basis: SalaryBasis.Net };

    service.series(filter).subscribe();
    controller.expectOne(`${base}/series?basis=Net`).flush({ hasEnoughData: true, points: [] });

    service.purchasingPower(filter).subscribe();
    controller.expectOne(`${base}/purchasing-power?basis=Net`).flush({ hasEnoughData: true, series: [] });

    service.insights(filter).subscribe();
    controller.expectOne(`${base}/insights?basis=Net`).flush({ hasEnoughData: true, insights: [] });
  });

  it('should request projection with horizon', () => {
    service.projection({ from: '2024-01', to: '2024-12', basis: SalaryBasis.Net }, 12).subscribe();
    const req = controller.expectOne(`${base}/projection?basis=Net&from=2024-01&to=2024-12&horizon=12`);
    expect(req.request.method).toBe('GET');
    expect(req.request.context.get(SKIP_ERROR_TOAST)).toBe(true);
    req.flush({ hasEnoughData: true, scenarios: [], series: [] });
  });
});
