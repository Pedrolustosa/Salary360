import { Competence } from './competence.interface';
import { SalarySeriesPoint } from './salary-series-point.interface';

export interface SalarySeries {
  hasEnoughData: boolean;
  requiredRecords: number;
  recordCount: number;
  from: Competence | null;
  to: Competence | null;
  points: SalarySeriesPoint[];
}
