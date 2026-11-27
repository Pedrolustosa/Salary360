import { InsightSeverity } from '../enums/insight-severity.enum';

export interface SalaryInsight {
  code: string;
  severity: InsightSeverity;
  params: Record<string, unknown>;
  referenceCompetence: string | null;
}
