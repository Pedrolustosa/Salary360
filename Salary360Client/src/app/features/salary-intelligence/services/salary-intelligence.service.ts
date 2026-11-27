import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { withoutErrorToast } from '../../../core/api/api-error';
import { SalaryBasis } from '../enums/salary-basis.enum';
import { PurchasingPower } from '../interfaces/purchasing-power.interface';
import { SalaryAnalysisFilter } from '../interfaces/salary-analysis-filter.interface';
import { SalaryInsights } from '../interfaces/salary-insights.interface';
import { SalaryIntelligenceSummary } from '../interfaces/salary-intelligence-summary.interface';
import { SalaryProjection } from '../interfaces/salary-projection.interface';
import { SalarySeries } from '../interfaces/salary-series.interface';

@Injectable({ providedIn: 'root' })
export class SalaryIntelligenceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/salary-intelligence`;

  summary(filter: SalaryAnalysisFilter): Observable<SalaryIntelligenceSummary> {
    return this.http.get<SalaryIntelligenceSummary>(`${this.baseUrl}/summary`, {
      params: this.params(filter),
      context: withoutErrorToast(),
    });
  }

  series(filter: SalaryAnalysisFilter): Observable<SalarySeries> {
    return this.http.get<SalarySeries>(`${this.baseUrl}/series`, {
      params: this.params(filter),
      context: withoutErrorToast(),
    });
  }

  purchasingPower(filter: SalaryAnalysisFilter): Observable<PurchasingPower> {
    return this.http.get<PurchasingPower>(`${this.baseUrl}/purchasing-power`, {
      params: this.params(filter),
      context: withoutErrorToast(),
    });
  }

  insights(filter: SalaryAnalysisFilter): Observable<SalaryInsights> {
    return this.http.get<SalaryInsights>(`${this.baseUrl}/insights`, {
      params: this.params(filter),
      context: withoutErrorToast(),
    });
  }

  projection(filter: SalaryAnalysisFilter, horizon: number): Observable<SalaryProjection> {
    return this.http.get<SalaryProjection>(`${this.baseUrl}/projection`, {
      params: this.params(filter).set('horizon', horizon),
      context: withoutErrorToast(),
    });
  }

  private params(filter: SalaryAnalysisFilter): HttpParams {
    let params = new HttpParams().set('basis', filter.basis ?? SalaryBasis.Net);
    if (filter.from) params = params.set('from', filter.from);
    if (filter.to) params = params.set('to', filter.to);
    return params;
  }
}
