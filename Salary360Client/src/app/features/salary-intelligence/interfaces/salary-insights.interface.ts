import { Competence } from './competence.interface';
import { SalaryInsight } from './salary-insight.interface';

export interface SalaryInsights {
  hasEnoughData: boolean;
  requiredRecords: number;
  recordCount: number;
  from: Competence | null;
  to: Competence | null;
  insights: SalaryInsight[];
}
