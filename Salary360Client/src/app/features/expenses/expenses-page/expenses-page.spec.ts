import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { Subject, of, throwError } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { SalaryService } from '../../salaries/services/salary.service';
import { ExpenseCategoryService } from '../services/expense-category.service';
import { ExpenseService } from '../services/expense.service';
import { ExpensesPage } from './expenses-page';

describe('ExpensesPage', () => {
  let fixture: ComponentFixture<ExpensesPage>;
  const expenseChanged$ = new Subject<void>();
  const salaryChanged$ = new Subject<void>();
  const categoryChanged$ = new Subject<void>();

  const expenses = {
    changed$: expenseChanged$.asObservable(),
    list: vi.fn(),
    summary: vi.fn(),
    series: vi.fn(),
    setPaid: vi.fn(),
    delete: vi.fn(),
    copy: vi.fn(),
    get: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
  };

  const categories = {
    changed$: categoryChanged$.asObservable(),
    list: vi.fn(),
  };

  const salaries = { changed$: salaryChanged$.asObservable() };
  const auth = { hasPermission: vi.fn(() => true) };
  const toastr = { success: vi.fn(), warning: vi.fn() };

  const emptySummary = {
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
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    vi.useFakeTimers();
    expenses.list.mockReturnValue(of({ items: [], total: 0, page: 1, pageSize: 20 }));
    expenses.summary.mockReturnValue(of(emptySummary));
    expenses.series.mockReturnValue(of([]));
    categories.list.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [ExpensesPage],
      providers: [
        provideTranslateService({ fallbackLang: 'pt-BR' }),
        provideRouter([]),
        LanguageService,
        { provide: ExpenseService, useValue: expenses },
        { provide: ExpenseCategoryService, useValue: categories },
        { provide: SalaryService, useValue: salaries },
        { provide: AuthService, useValue: auth },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ExpensesPage);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should show empty list state', async () => {
    fixture.detectChanges();
    await vi.advanceTimersByTimeAsync(300);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('expenses.empty');
  });

  it('should show list data', async () => {
    expenses.list.mockReturnValue(
      of({
        items: [
          {
            id: 'e1',
            categoryId: 'c1',
            categoryName: 'Food',
            categorySystemKey: null,
            categoryColorKey: 'Orange',
            categoryIconKey: 'ShoppingBag',
            name: 'Market',
            type: 'Variable',
            amount: 50,
            year: 2024,
            month: 1,
            dueDay: 5,
            isPaid: false,
            paidAtUtc: null,
            seriesId: null,
          },
        ],
        total: 1,
        page: 1,
        pageSize: 20,
      }),
    );
    fixture = TestBed.createComponent(ExpensesPage);
    fixture.detectChanges();
    await vi.advanceTimersByTimeAsync(300);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Market');
  });

  it('should show summary error state', async () => {
    expenses.summary.mockReturnValue(throwError(() => new Error('fail')));
    fixture = TestBed.createComponent(ExpensesPage);
    fixture.detectChanges();
    await vi.advanceTimersByTimeAsync(300);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('expenses.states.error');
  });

  it('should reload when salary changes', async () => {
    fixture.detectChanges();
    await vi.advanceTimersByTimeAsync(300);
    expenses.summary.mockClear();
    salaryChanged$.next();
    await vi.advanceTimersByTimeAsync(300);
    expect(expenses.summary).toHaveBeenCalled();
  });
});
