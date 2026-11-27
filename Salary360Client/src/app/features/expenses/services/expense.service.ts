import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, Subject, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ExpenseChangeScope } from '../enums/expense-change-scope.enum';
import { CopyExpensesRequest } from '../interfaces/copy-expenses-request.interface';
import { CopyExpensesResult } from '../interfaces/copy-expenses-result.interface';
import { CreateExpenseRequest } from '../interfaces/create-expense-request.interface';
import { Expense } from '../interfaces/expense.interface';
import { ExpenseMonthSummary } from '../interfaces/expense-month-summary.interface';
import { ExpenseSeriesPoint } from '../interfaces/expense-series-point.interface';
import { ListExpensesQuery } from '../interfaces/list-expenses-query.interface';
import { PagedExpenses } from '../interfaces/paged-expenses.interface';
import { UpdateExpenseRequest } from '../interfaces/update-expense-request.interface';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/expenses`;
  private readonly changedSubject = new Subject<void>();

  readonly changed$: Observable<void> = this.changedSubject.asObservable();

  list(query: ListExpensesQuery): Observable<PagedExpenses> {
    let params = new HttpParams()
      .set('year', query.year)
      .set('month', query.month)
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);
    if (query.categoryId) params = params.set('categoryId', query.categoryId);
    if (query.type) params = params.set('type', query.type);
    if (query.isPaid !== null && query.isPaid !== undefined) params = params.set('isPaid', query.isPaid);
    return this.http.get<PagedExpenses>(this.baseUrl, { params });
  }

  get(id: string): Observable<Expense> {
    return this.http.get<Expense>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateExpenseRequest): Observable<Expense[]> {
    return this.http.post<Expense[]>(this.baseUrl, request).pipe(tap(() => this.changedSubject.next()));
  }

  update(id: string, request: UpdateExpenseRequest, scope: ExpenseChangeScope): Observable<Expense> {
    const params = new HttpParams().set('scope', scope);
    return this.http
      .put<Expense>(`${this.baseUrl}/${id}`, request, { params })
      .pipe(tap(() => this.changedSubject.next()));
  }

  delete(id: string, scope: ExpenseChangeScope = ExpenseChangeScope.This): Observable<void> {
    const params = new HttpParams().set('scope', scope);
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { params }).pipe(tap(() => this.changedSubject.next()));
  }

  setPaid(id: string, isPaid: boolean): Observable<Expense> {
    return this.http
      .patch<Expense>(`${this.baseUrl}/${id}/paid`, { isPaid })
      .pipe(tap(() => this.changedSubject.next()));
  }

  copy(request: CopyExpensesRequest): Observable<CopyExpensesResult> {
    return this.http.post<CopyExpensesResult>(`${this.baseUrl}/copy`, request).pipe(tap(() => this.changedSubject.next()));
  }

  summary(year: number, month: number): Observable<ExpenseMonthSummary> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<ExpenseMonthSummary>(`${this.baseUrl}/summary`, { params });
  }

  series(fromYear: number, fromMonth: number, toYear: number, toMonth: number): Observable<ExpenseSeriesPoint[]> {
    const params = new HttpParams()
      .set('fromYear', fromYear)
      .set('fromMonth', fromMonth)
      .set('toYear', toYear)
      .set('toMonth', toMonth);
    return this.http.get<ExpenseSeriesPoint[]>(`${this.baseUrl}/series`, { params });
  }
}
