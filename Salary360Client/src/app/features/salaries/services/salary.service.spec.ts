import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { EvolutionMode } from '../enums/evolution-mode.enum';
import { SalaryItemType } from '../enums/salary-item-type.enum';
import { SalaryService } from './salary.service';

describe('SalaryService', () => {
  let service: SalaryService;
  let controller: HttpTestingController;
  const base = `${environment.apiUrl}/salaries`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SalaryService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should list salaries with optional year filter', () => {
    service.list(2024, 2, 10).subscribe();
    const req = controller.expectOne(`${base}?page=2&pageSize=10&year=2024`);
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], page: 2, pageSize: 10, totalCount: 0 });
  });

  it('should get, create, update and delete salaries', () => {
    let changeCount = 0;
    service.changed$.subscribe(() => {
      changeCount += 1;
    });

    service.get('id-1').subscribe();
    controller.expectOne(`${base}/id-1`).flush({ id: 'id-1' });
    expect(changeCount).toBe(0);

    service
      .create({
        year: 2024,
        month: 1,
        grossAmount: 1000,
        notes: null,
        items: [{ name: 'VR', type: SalaryItemType.Benefit, amount: 100 }],
      })
      .subscribe();
    const create = controller.expectOne(base);
    expect(create.request.method).toBe('POST');
    create.flush({ id: 'id-1' });
    expect(changeCount).toBe(1);

    service
      .update('id-1', {
        year: 2024,
        month: 1,
        grossAmount: 1100,
        notes: null,
        items: [],
      })
      .subscribe();
    const update = controller.expectOne(`${base}/id-1`);
    expect(update.request.method).toBe('PUT');
    update.flush({ id: 'id-1' });
    expect(changeCount).toBe(2);

    service.delete('id-1').subscribe();
    const del = controller.expectOne(`${base}/id-1`);
    expect(del.request.method).toBe('DELETE');
    del.flush(null);
    expect(changeCount).toBe(3);
  });


  it('should request evolution with mode filters', () => {
    service.evolution({ mode: EvolutionMode.Month, month: 9, fromYear: 2020, toYear: 2024 }).subscribe();
    const req = controller.expectOne(`${base}/evolution?mode=Month&fromYear=2020&toYear=2024&month=9`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
