import { ChangeDetectionStrategy, Component, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { MonthYearPicker } from '../../../shared/month-year-picker/month-year-picker';
import { MonthYearValue } from '../../../shared/month-year-picker/interfaces/month-year-value.interface';
import { SalaryBasis } from '../enums/salary-basis.enum';
import { SalaryAnalysisFilter } from '../interfaces/salary-analysis-filter.interface';
import {
  allPeriod,
  currentYearPeriod,
  formatCompetence,
  last12Months,
  parseCompetence,
  validatePeriod,
} from '../utils/salary-intelligence-filters';

@Component({
  selector: 'app-salary-analysis-filters',
  imports: [FormsModule, TranslatePipe, MonthYearPicker],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './salary-analysis-filters.html',
  styleUrl: './salary-analysis-filters.scss',
})
export class SalaryAnalysisFilters {
  readonly value = input.required<SalaryAnalysisFilter>();
  readonly filterChange = output<SalaryAnalysisFilter>();

  protected readonly SalaryBasis = SalaryBasis;
  protected readonly filterError = signal<string | null>(null);
  protected readonly fromValue = signal<MonthYearValue | null>(null);
  protected readonly toValue = signal<MonthYearValue | null>(null);
  protected readonly basis = signal(SalaryBasis.Net);

  constructor() {
    effect(() => {
      const filter = this.value();
      this.fromValue.set(parseCompetence(filter.from));
      this.toValue.set(parseCompetence(filter.to));
      this.basis.set(filter.basis);
      this.filterError.set(validatePeriod(filter.from, filter.to));
    });
  }

  protected setBasis(basis: SalaryBasis): void {
    this.basis.set(basis);
    this.emitCurrent();
  }

  protected onFromChange(value: MonthYearValue | null): void {
    this.fromValue.set(value);
    this.emitCurrent();
  }

  protected onToChange(value: MonthYearValue | null): void {
    this.toValue.set(value);
    this.emitCurrent();
  }

  protected applyLast12(): void {
    const period = last12Months();
    this.fromValue.set(parseCompetence(period.from));
    this.toValue.set(parseCompetence(period.to));
    this.emitCurrent();
  }

  protected applyCurrentYear(): void {
    const period = currentYearPeriod();
    this.fromValue.set(parseCompetence(period.from));
    this.toValue.set(parseCompetence(period.to));
    this.emitCurrent();
  }

  protected applyAll(): void {
    const period = allPeriod();
    this.fromValue.set(parseCompetence(period.from));
    this.toValue.set(parseCompetence(period.to));
    this.emitCurrent();
  }

  private emitCurrent(): void {
    const from = this.fromValue();
    const to = this.toValue();
    const filter: SalaryAnalysisFilter = {
      from: from ? formatCompetence(from) : null,
      to: to ? formatCompetence(to) : null,
      basis: this.basis(),
    };
    const error = validatePeriod(filter.from, filter.to);
    this.filterError.set(error);
    if (error) return;
    this.filterChange.emit(filter);
  }
}
