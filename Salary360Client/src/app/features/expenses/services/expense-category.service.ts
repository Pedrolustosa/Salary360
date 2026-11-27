import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, Subject, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateDefaultsResult } from '../interfaces/create-defaults-result.interface';
import { CreateExpenseCategoryRequest } from '../interfaces/create-expense-category-request.interface';
import { ExpenseCategory } from '../interfaces/expense-category.interface';
import { ExpenseCategoryListItem } from '../interfaces/expense-category-list-item.interface';
import { UpdateExpenseCategoryRequest } from '../interfaces/update-expense-category-request.interface';

@Injectable({ providedIn: 'root' })
export class ExpenseCategoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/expense-categories`;
  private readonly changedSubject = new Subject<void>();

  readonly changed$: Observable<void> = this.changedSubject.asObservable();

  list(): Observable<ExpenseCategoryListItem[]> {
    return this.http.get<ExpenseCategoryListItem[]>(this.baseUrl);
  }

  create(request: CreateExpenseCategoryRequest): Observable<ExpenseCategory> {
    return this.http.post<ExpenseCategory>(this.baseUrl, request).pipe(tap(() => this.changedSubject.next()));
  }

  update(id: string, request: UpdateExpenseCategoryRequest): Observable<ExpenseCategory> {
    return this.http
      .put<ExpenseCategory>(`${this.baseUrl}/${id}`, request)
      .pipe(tap(() => this.changedSubject.next()));
  }

  delete(id: string, reassignTo?: string | null): Observable<void> {
    let params = new HttpParams();
    if (reassignTo) params = params.set('reassignTo', reassignTo);
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { params }).pipe(tap(() => this.changedSubject.next()));
  }

  createDefaults(): Observable<CreateDefaultsResult> {
    return this.http
      .post<CreateDefaultsResult>(`${this.baseUrl}/defaults`, {})
      .pipe(tap(() => this.changedSubject.next()));
  }
}
