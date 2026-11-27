import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { ExpenseChangeScope } from '../enums/expense-change-scope.enum';
import { ExpenseRecurrence } from '../enums/expense-recurrence.enum';
import { ExpenseType } from '../enums/expense-type.enum';
import { ExpenseService } from './expense.service';

describe('ExpenseService', () => {
  let service: ExpenseService;
  let controller: HttpTestingController;
  const base = `${environment.apiUrl}/expenses`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ExpenseService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should list expenses with filters', () => {
    service
      .list({ year: 2024, month: 5, categoryId: 'c1', type: ExpenseType.Fixed, isPaid: false, page: 2, pageSize: 10 })
      .subscribe();
    const req = controller.expectOne(
      `${base}?year=2024&month=5&page=2&pageSize=10&categoryId=c1&type=Fixed&isPaid=false`,
    );
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0, page: 2, pageSize: 10 });
  });

  it('should create update delete and set paid with changed$', () => {
    let changes = 0;
    service.changed$.subscribe(() => {
      changes += 1;
    });

    service
      .create({
        categoryId: 'c1',
        name: 'Rent',
        type: ExpenseType.Fixed,
        amount: 100,
        year: 2024,
        month: 1,
        dueDay: 10,
        isPaid: false,
        notes: null,
        recurrence: ExpenseRecurrence.None,
        recurrenceCount: null,
      })
      .subscribe();
    controller.expectOne(base).flush([{ id: 'e1' }]);
    expect(changes).toBe(1);

    service
      .update(
        'e1',
        {
          categoryId: 'c1',
          name: 'Rent',
          type: ExpenseType.Fixed,
          amount: 110,
          year: 2024,
          month: 1,
          dueDay: 10,
          isPaid: true,
          notes: null,
        },
        ExpenseChangeScope.This,
      )
      .subscribe();
    const update = controller.expectOne(`${base}/e1?scope=This`);
    expect(update.request.method).toBe('PUT');
    update.flush({ id: 'e1' });
    expect(changes).toBe(2);

    service.setPaid('e1', true).subscribe();
    const paid = controller.expectOne(`${base}/e1/paid`);
    expect(paid.request.method).toBe('PATCH');
    paid.flush({ id: 'e1' });
    expect(changes).toBe(3);

    service.delete('e1', ExpenseChangeScope.All).subscribe();
    const del = controller.expectOne(`${base}/e1?scope=All`);
    expect(del.request.method).toBe('DELETE');
    del.flush(null);
    expect(changes).toBe(4);
  });

  it('should request summary series and copy', () => {
    service.summary(2024, 3).subscribe();
    controller.expectOne(`${base}/summary?year=2024&month=3`).flush({
      totalFixed: 0,
      totalVariable: 0,
      total: 0,
      totalPaid: 0,
      totalPending: 0,
      count: 0,
      byCategory: [],
      hasSalary: false,
      netAmount: null,
      committedPercent: null,
      available: null,
    });

    service.series(2024, 1, 2024, 12).subscribe();
    controller.expectOne(`${base}/series?fromYear=2024&fromMonth=1&toYear=2024&toMonth=12`).flush([]);

    service.copy({ fromYear: 2024, fromMonth: 1, toYear: 2024, toMonth: 2, onlyFixed: true }).subscribe();
    const copy = controller.expectOne(`${base}/copy`);
    expect(copy.request.method).toBe('POST');
    copy.flush({ created: 1, skipped: 0 });
  });
});
