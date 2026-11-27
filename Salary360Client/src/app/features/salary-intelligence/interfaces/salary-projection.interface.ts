import { Competence } from './competence.interface';
import { ProjectionPoint } from './projection-point.interface';
import { ProjectionScenario } from './projection-scenario.interface';

export interface SalaryProjection {
  hasEnoughData: boolean;
  requiredRecords: number;
  recordCount: number;
  from: Competence | null;
  to: Competence | null;
  method: string | null;
  sampleSize: number | null;
  horizonMonths: number | null;
  disclaimerCode: string | null;
  inflationCriterion: string | null;
  scenarios: ProjectionScenario[];
  series: ProjectionPoint[];
}
