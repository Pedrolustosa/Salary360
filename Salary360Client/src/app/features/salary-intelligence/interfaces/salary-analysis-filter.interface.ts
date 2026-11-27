import { SalaryBasis } from '../enums/salary-basis.enum';

export interface SalaryAnalysisFilter {
  from: string | null;
  to: string | null;
  basis: SalaryBasis;
}
