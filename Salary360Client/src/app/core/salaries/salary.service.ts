import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, Subject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EvolutionMode } from './enums/evolution-mode.enum';
import { CreateSalaryRequest } from './interfaces/create-salary-request.interface';
import { PagedSalaries } from './interfaces/paged-salaries.interface';
import { SalaryEvolutionPoint } from './interfaces/salary-evolution-point.interface';
import { SalaryEvolutionQuery } from './interfaces/salary-evolution-query.interface';
import { SalaryRecord } from './interfaces/salary-record.interface';
import { UpdateSalaryRequest } from './interfaces/update-salary-request.interface';

@Injectable({ providedIn: 'root' })
export class SalaryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/salaries`;
  private readonly changedSubject = new Subject<void>();

  readonly changed$: Observable<void> = this.changedSubject.asObservable();

  list(year?: number | null, page = 1, pageSize = 20): Observable<PagedSalaries> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (year !== null && year !== undefined) params = params.set('year', year);
    return this.http.get<PagedSalaries>(this.baseUrl, { params });
  }

  get(id: string): Observable<SalaryRecord> {
    return this.http.get<SalaryRecord>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateSalaryRequest): Observable<SalaryRecord> {
    return this.http.post<SalaryRecord>(this.baseUrl, request).pipe(tap(() => this.changedSubject.next()));
  }

  update(id: string, request: UpdateSalaryRequest): Observable<SalaryRecord> {
    return this.http.put<SalaryRecord>(`${this.baseUrl}/${id}`, request).pipe(tap(() => this.changedSubject.next()));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(tap(() => this.changedSubject.next()));
  }

  evolution(query: SalaryEvolutionQuery): Observable<SalaryEvolutionPoint[]> {
    let params = new HttpParams()
      .set('mode', query.mode)
      .set('fromYear', query.fromYear)
      .set('toYear', query.toYear);
    if (query.mode === EvolutionMode.Month && query.month != null) params = params.set('month', query.month);
    return this.http.get<SalaryEvolutionPoint[]>(`${this.baseUrl}/evolution`, { params });
  }
}
