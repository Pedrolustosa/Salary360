import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { CategoryColor } from '../enums/category-color.enum';
import { CategoryIcon } from '../enums/category-icon.enum';
import { ExpenseCategoryService } from './expense-category.service';

describe('ExpenseCategoryService', () => {
  let service: ExpenseCategoryService;
  let controller: HttpTestingController;
  const base = `${environment.apiUrl}/expense-categories`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ExpenseCategoryService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('should list create update delete and defaults with changed$', () => {
    let changes = 0;
    service.changed$.subscribe(() => {
      changes += 1;
    });

    service.list().subscribe();
    controller.expectOne(base).flush([]);
    expect(changes).toBe(0);

    service.create({ name: 'Gym', colorKey: CategoryColor.Pink, iconKey: CategoryIcon.Heart }).subscribe();
    const create = controller.expectOne(base);
    expect(create.request.method).toBe('POST');
    create.flush({ id: 'c1' });
    expect(changes).toBe(1);

    service
      .update('c1', { name: 'Gym', colorKey: CategoryColor.Red, iconKey: CategoryIcon.Heart })
      .subscribe();
    const update = controller.expectOne(`${base}/c1`);
    expect(update.request.method).toBe('PUT');
    update.flush({ id: 'c1' });
    expect(changes).toBe(2);

    service.delete('c1', 'c2').subscribe();
    const del = controller.expectOne(`${base}/c1?reassignTo=c2`);
    expect(del.request.method).toBe('DELETE');
    del.flush(null);
    expect(changes).toBe(3);

    service.createDefaults().subscribe();
    const defaults = controller.expectOne(`${base}/defaults`);
    expect(defaults.request.method).toBe('POST');
    defaults.flush({ created: 10 });
    expect(changes).toBe(4);
  });
});
